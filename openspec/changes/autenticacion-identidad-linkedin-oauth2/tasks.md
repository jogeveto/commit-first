# Tasks — autenticacion-identidad-linkedin-oauth2

> TDD por sub-slice (red → green → refactor). `journey_smoke` verde entre cada sub-slice antes del siguiente. Verificación **ejecutada** (tests + docker), no por inspección.

## SS-001-a · Backend OAuth2 + provisión de cuenta (HU-001, HU-002)

- [ ] 1.1 Definir `ILinkedInClient` + `LinkedInProfile` (sub, nombre) y `FakeLinkedInClient` determinista (DI).
- [ ] 1.2 (test-first) Test: callback con `state` válido + code mock → sesión emitida + `User_ID`. (HU-001 AC1)
- [ ] 1.3 (test-first) Test: consentimiento rechazado / error de LinkedIn → sin sesión, mensaje. (HU-001 AC2)
- [ ] 1.4 (test-first) Test: `state` ausente/mismatch → rechazo, sin sesión, intento registrado. (HU-001 AC3)
- [ ] 1.5 Implementar endpoints `/auth/linkedin/start` (genera `state`) y `/auth/linkedin/callback` (valida `state`, usa `ILinkedInClient`).
- [ ] 1.6 (test-first) Test: primer acceso crea `users` con `User_ID` nuevo. (HU-002 AC1)
- [ ] 1.7 (test-first) Test: fallo de persistencia → rollback, sin sesión, sin cuenta parcial. (HU-002 AC2)
- [ ] 1.8 (test-first) Test: acceso recurrente (mismo `linkedin_sub`) reutiliza `User_ID`, no duplica. (HU-002 AC3)
- [ ] 1.9 Implementar provisión idempotente por `linkedin_sub` (transacción) + emisión de JWT.
- [ ] 1.10 journey_smoke SS-a: `docker compose up` + POST callback mock retorna token y persiste `users` (verificado con curl/cliente HTTP). Marcar `EP-001-a.status=done` solo con smoke verde.

## SS-001-b · Middleware de sesión + endpoints protegidos (HU-003)

- [ ] 2.1 (test-first) Test: petición con token válido → middleware resuelve `User_ID`, 200 con datos. (HU-003 AC1)
- [ ] 2.2 (test-first) Test: token ausente/manipulado → 401, sin operación. (HU-003 AC2)
- [ ] 2.3 (test-first) Test: token expirado → 401 con re-auth, sin exponer datos. (HU-003 AC3)
- [ ] 2.4 Implementar middleware de validación JWT + endpoint protegido de prueba (`/api/me` retorna `User_ID`).
- [ ] 2.5 journey_smoke SS-b: llamada autenticada 200 / no autenticada 401 en el stack levantado.

## SS-001-c · Login React + cableado end-to-end (HU-001 UI)

- [ ] 3.1 Componente de login (botón "Continuar con LinkedIn") → redirige a `/auth/linkedin/start`.
- [ ] 3.2 Manejo del callback en el SPA + almacenamiento de sesión + redirección al área autenticada.
- [ ] 3.3 Cableado real al backend (SS-a + SS-b corriendo); llamada protegida `/api/me` con la sesión.
- [ ] 3.4 Fidelidad: screenshot de la pantalla de login vs `docs/07-prototipo/` vía MCP chrome-devtools (gate `fidelity`).
- [ ] 3.5 journey_smoke SS-c end-to-end: clic login → flujo OAuth2 (mock) → área autenticada → `/api/me` 200.

## Cierre del slice

- [ ] 4.1 Stack-guard: toda dependencia nueva (auth handlers, Npgsql/EF) está en `stack-allowlist.json` con justificación.
- [ ] 4.2 Secreto: verificar que el client secret / JWT key nunca llegan al bundle del frontend (WC-006).
- [ ] 4.3 wiring_verified: verificación adversarial independiente (contexto virgen) de los 12 items del `wiring_checklist`.
- [ ] 4.4 DoD (reducido, inner loop) verde → PR a `main` + archivar el change en el mismo PR.
