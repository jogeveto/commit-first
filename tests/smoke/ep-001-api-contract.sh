#!/usr/bin/env bash
# Contrato de API de EP-001 (autenticación e identidad) — regresión repetible.
#
# Sin dependencias de paquetes: curl para HTTP y `docker exec ... psql` para las
# comprobaciones de base de datos. Se prefirió esto a una colección Newman/Postman
# porque Newman no está en `.claude/config/stack-allowlist.json` y el valor de
# regresión es el mismo. Cubre los AC verificables por HTTP; HU-002 AC2 (fallo de
# persistencia) se cubre en los tests unitarios (FailingUserRepository), no aquí.
#
# Uso:  bash tests/smoke/ep-001-api-contract.sh [base_url]
#       (requiere el stack levantado: docker compose up -d)
set -uo pipefail

BASE="${1:-http://localhost:8080}"
PASS=0; FAIL=0

# Identidad distinta por ejecución: sin esto, los checks contra la base de datos
# pueden pasar sobre filas dejadas por corridas anteriores (verde falso).
CORRIDA="$(date +%s)-$$"
SUB_HAPPY="contract-happy-$CORRIDA"

check() { # check <descripción> <esperado> <obtenido>
  if [ "$2" = "$3" ]; then
    PASS=$((PASS+1)); printf '  ✓ %s\n' "$1"
  else
    FAIL=$((FAIL+1)); printf '  ✗ %s\n     esperado: %s\n     obtenido: %s\n' "$1" "$2" "$3"
  fi
}

contains() { # contains <descripción> <aguja> <pajar>
  case "$3" in
    *"$2"*) PASS=$((PASS+1)); printf '  ✓ %s\n' "$1" ;;
    *) FAIL=$((FAIL+1)); printf '  ✗ %s\n     no contiene: %s\n     en: %s\n' "$1" "$2" "$3" ;;
  esac
}

# Emite un `state` fresco desde /auth/linkedin/start y lo devuelve.
nuevo_state() {
  curl -si "$BASE/auth/linkedin/start" \
    | grep -i '^location:' | grep -oiE 'state=[A-F0-9]+' | cut -d= -f2 | tr -d '\r'
}

echo "== EP-001 · contrato de API ($BASE) =="

# GUARDA DE MODO. El contrato ejercita el flujo OAuth2 con un `code` simulado, lo
# cual solo es posible con FakeLinkedInClient. Si el backend corre con credenciales
# reales, /auth/linkedin/start redirige a linkedin.com y estos checks no pueden
# pasar: hay que ABORTAR con un mensaje claro en vez de reportar fallos (o, peor,
# verdes engañosos). Para correrlo, levanta el backend en modo mock:
#   LINKEDIN_CLIENT_ID= LINKEDIN_CLIENT_SECRET= docker compose up -d backend
LOC_MODO=$(curl -si "$BASE/auth/linkedin/start" | grep -i '^location:' | tr -d '\r')
case "$LOC_MODO" in
  *linkedin.com*)
    echo "ABORTADO: el backend corre con credenciales REALES de LinkedIn."
    echo "  /auth/linkedin/start → linkedin.com (no se puede simular el consentimiento)."
    echo "  Para el contrato, levanta el backend en modo mock:"
    echo "    LINKEDIN_CLIENT_ID= LINKEDIN_CLIENT_SECRET= docker compose up -d backend"
    exit 2 ;;
  *) echo "(modo mock detectado — se puede simular el flujo OAuth2)" ;;
esac

echo "-- scaffold --"
check "GET /health → 200" "200" "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/health")"

echo "-- HU-001 · inicio del flujo (CSRF) --"
LOC=$(curl -si "$BASE/auth/linkedin/start" | grep -i '^location:' | tr -d '\r')
check "GET /auth/linkedin/start → 302" "302" "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/auth/linkedin/start")"
contains "Location emite un state CSRF" "state=" "$LOC"

echo "-- HU-001 AC1 / HU-002 AC1 · happy path --"
S=$(nuevo_state)
OK=$(curl -s "$BASE/auth/linkedin/callback?code=$SUB_HAPPY&state=$S&format=json")
contains "callback exitoso → outcome success" '"outcome":"success"' "$OK"
contains "callback exitoso → emite token de sesión" '"token":"' "$OK"
USER1=$(printf '%s' "$OK" | grep -oE '"userId":"[^"]+"' | cut -d'"' -f4)
[ -n "$USER1" ] && { PASS=$((PASS+1)); echo "  ✓ callback exitoso → User_ID presente"; } \
                || { FAIL=$((FAIL+1)); echo "  ✗ callback exitoso → falta User_ID"; }

echo "-- HU-002 AC3 · idempotencia (mismo sub no duplica) --"
S=$(nuevo_state)
USER2=$(curl -s "$BASE/auth/linkedin/callback?code=$SUB_HAPPY&state=$S&format=json" \
  | grep -oE '"userId":"[^"]+"' | cut -d'"' -f4)
# Comparar dos cadenas VACÍAS daría verde aunque ambos logins hubieran fallado
# (verde falso detectado en auditoría): se exige que el User_ID exista.
if [ -n "$USER1" ] && [ "$USER1" = "$USER2" ]; then
  PASS=$((PASS+1)); echo "  ✓ acceso recurrente reutiliza el User_ID"
else
  FAIL=$((FAIL+1)); echo "  ✗ acceso recurrente reutiliza el User_ID (1º='$USER1' 2º='$USER2')"
fi

# La igualdad de User_ID es evidencia indirecta; esto comprueba la FILA REAL creada
# POR ESTA CORRIDA (el `sub` lleva un sufijo único: con uno fijo, el check pasaba
# sobre una fila residual de ejecuciones anteriores aunque no ocurriera ningún login).
if command -v docker >/dev/null 2>&1; then
  FILAS=$(docker exec "${DB_CONTAINER:-my-top-profile-db-1}" \
    psql -U "${DB_USER:-empleo}" -d "${DB_NAME:-empleabilidad}" -tAc \
    "select count(*) from users where linkedin_sub='linkedin-sub-$SUB_HAPPY'" 2>/dev/null | tr -d '[:space:]')
  check "tras dos logins hay exactamente 1 fila nueva en users" "1" "$FILAS"
else
  FAIL=$((FAIL+1)); echo "  ✗ docker no disponible: NO se pudo verificar la fila en users"
fi

echo "-- HU-001 AC3 · state inválido (CSRF) --"
check "state forjado → 401" "401" \
  "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/auth/linkedin/callback?code=x&state=FORJADO&format=json")"
check "state ausente → 401" "401" \
  "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/auth/linkedin/callback?code=x&format=json")"

echo "-- HU-001 AC3 · state de un solo uso (anti-replay) --"
S=$(nuevo_state)
curl -s -o /dev/null "$BASE/auth/linkedin/callback?code=contract-replay&state=$S&format=json"
contains "reusar el mismo state → invalid_state" '"outcome":"invalid_state"' \
  "$(curl -s "$BASE/auth/linkedin/callback?code=contract-replay&state=$S&format=json")"

echo "-- HU-001 AC2 · consentimiento rechazado --"
S=$(nuevo_state)
RECH=$(curl -s "$BASE/auth/linkedin/callback?state=$S&error=access_denied&format=json")
contains "consentimiento rechazado → consent_rejected" '"outcome":"consent_rejected"' "$RECH"
contains "consentimiento rechazado → mensaje explicable" '"message":"' "$RECH"

echo "-- HU-001 AC1 · el callback devuelve al SPA (modo redirect) --"
S=$(nuevo_state)
RLOC=$(curl -si "$BASE/auth/linkedin/callback?code=contract-redirect&state=$S" | grep -i '^location:' | tr -d '\r')
contains "éxito → redirige a /auth/callback del SPA" "/auth/callback" "$RLOC"
contains "el token viaja en el FRAGMENTO, no en el query" "#token=" "$RLOC"

# Rama de NAVEGADOR de los errores: es la que recorre un usuario real (la de
# ?format=json solo la usan las pruebas). Sin estos checks quedaría sin regresión.
echo "-- HU-001 AC2/AC3 · errores en la rama de navegador (redirect al login) --"
S=$(nuevo_state)
ELOC=$(curl -si "$BASE/auth/linkedin/callback?state=$S&error=access_denied" | grep -i '^location:' | tr -d '\r')
contains "consentimiento rechazado → vuelve a /login" "/login" "$ELOC"
contains "consentimiento rechazado → informa el motivo" "error=consent_rejected" "$ELOC"
contains "consentimiento rechazado → lleva mensaje para el usuario" "message=" "$ELOC"

CLOC=$(curl -si "$BASE/auth/linkedin/callback?code=x&state=FORJADO" | grep -i '^location:' | tr -d '\r')
contains "state inválido → vuelve a /login" "/login" "$CLOC"
contains "state inválido → informa el motivo" "error=invalid_state" "$CLOC"
check "ningún redirect de error lleva sesión" "0" \
  "$(printf '%s\n%s\n' "$ELOC" "$CLOC" | grep -c 'token=' || true)"

echo "-- HU-003 · endpoint protegido --"
S=$(nuevo_state)
TOKEN=$(curl -s "$BASE/auth/linkedin/callback?code=contract-me&state=$S&format=json" \
  | grep -oE '"token":"[^"]+"' | cut -d'"' -f4)
check "GET /api/me con token válido → 200" "200" \
  "$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer $TOKEN" "$BASE/api/me")"
contains "/api/me resuelve el User_ID de la sesión" '"userId":"' \
  "$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE/api/me")"
check "GET /api/me sin token → 401" "401" "$(curl -s -o /dev/null -w '%{http_code}' "$BASE/api/me")"
check "GET /api/me con token manipulado → 401" "401" \
  "$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer ${TOKEN}XXXX" "$BASE/api/me")"
contains "401 anuncia el esquema Bearer" "WWW-Authenticate: Bearer" \
  "$(curl -si "$BASE/api/me" | tr -d '\r')"

echo
echo "== resultado: $PASS OK, $FAIL fallos =="
[ "$FAIL" -eq 0 ] || exit 1
