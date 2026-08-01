## Context

Primer slice de construcción (EP-001, foundational). Establece identidad + sesión sobre el scaffold containerizado ya existente (backend .NET 8, PostgreSQL con tabla `users`, frontend React). No hay credenciales reales de LinkedIn en el entorno → se construye **mock-first** tras una frontera de servicio, con el comportamiento del sistema 100% verificable y la integración real activable por configuración.

## Goals

- Login LinkedIn OAuth2 sin contraseñas, con `state` CSRF y client secret server-side.
- Provisión de cuenta idempotente por `linkedin_sub`.
- Sesión JWT ligada al `User_ID` + middleware de autorización (401 en fallos).
- Pantalla de login React fiel al prototipo, cableada end-to-end.
- Todo verificable por ejecución (tests + docker smoke), sin credenciales reales.

## Decisions

### D1 — Frontera `ILinkedInClient` (mock-first)
La integración con LinkedIn se aísla tras `ILinkedInClient` (contrato del dominio declarado en el bloque de dominio de `CLAUDE.md`). Dos implementaciones:
- `FakeLinkedInClient` (dev/test): dado un `code`, retorna un `LinkedInProfile` determinista (`sub`, nombre). Habilita verificar los AC sin red ni credenciales.
- `RealLinkedInClient` (producción): intercambia el `code` por token y consulta el perfil real. Se inyecta cuando `LINKEDIN_CLIENT_ID/SECRET` están presentes.
La selección es por configuración (DI), sin cambiar código de dominio. Trade-off: un punto de integración real queda sin ejercitar hasta tener credenciales → registrado en `wiring_checklist` (WC-002) como pendiente de verificación con proveedor real.

### D2 — `state` CSRF
Al iniciar el flujo se genera un `state` aleatorio firmado/almacenado (cookie httpOnly o cache server-side con TTL). El callback exige coincidencia exacta; ausencia/mismatch → rechazo sin sesión (HU-001 AC3).

### D3 — Provisión idempotente
`users.linkedin_sub UNIQUE` (ya en el esquema del scaffold). Alta en una transacción; `INSERT ... ON CONFLICT (linkedin_sub) DO NOTHING` + `SELECT` del `User_ID`, o patrón equivalente en EF Core. Fallo de persistencia → rollback, sin sesión, sin cuenta parcial (HU-002 AC2).

### D4 — Sesión JWT + middleware
JWT firmado con `JWT_SIGNING_KEY` (server-side), claim `sub = User_ID`, expiración configurable. Middleware valida firma + expiración e inyecta `User_ID` en el contexto; ausencia/inválido/expirado → 401 (HU-003). El `User_ID` del contexto será la clave de tenancy que consumirá EP-002.

### D5 — Frontend
Pantalla de login (botón "Continuar con LinkedIn") que redirige a `/auth/linkedin/start`; el callback deja la sesión y el SPA redirige al área autenticada. Fidelidad verificada contra `docs/07-prototipo/` vía MCP chrome-devtools en la fase smoke del sub-slice C.

## Risks / Trade-offs

- **Sin credenciales reales**: el path real de LinkedIn no se ejercita aún (mitigado por la frontera + WC-002; no bloquea el DoD del comportamiento del sistema).
- **Alcance de scopes de LinkedIn**: qué campos del perfil se obtienen depende del scope real (spike documentado en HU-007/EP-003; aquí solo se necesita `sub` + nombre).
- **Dependencias nuevas** (auth handlers, Npgsql/EF): validar contra `stack-allowlist.json` (hook `stack-guard.sh`) al agregarlas.

## Migration
No aplica (primera capacidad; sin comportamiento previo que migrar).
