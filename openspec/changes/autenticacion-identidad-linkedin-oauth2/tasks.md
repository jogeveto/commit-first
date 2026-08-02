# Tasks — autenticacion-identidad-linkedin-oauth2

> TDD por sub-slice (red → green → refactor). `journey_smoke` verde entre cada sub-slice antes del siguiente. Verificación **ejecutada** (tests + docker), no por inspección.

## SS-001-a · Backend OAuth2 + provisión de cuenta (HU-001, HU-002) — ✅ DONE

- [x] 1.1 Definir `ILinkedInClient` + `LinkedInProfile` (sub, nombre) y `FakeLinkedInClient` determinista (DI). — `Fake`+`Real` tras frontera, seleccionados por config.
- [x] 1.2 (test-first) Test: callback con `state` válido + code mock → sesión emitida + `User_ID`. (HU-001 AC1) — `LinkedInAuthenticationServiceTests` RED→GREEN.
- [x] 1.3 (test-first) Test: consentimiento rechazado / error de LinkedIn → sin sesión, mensaje. (HU-001 AC2)
- [x] 1.4 (test-first) Test: `state` ausente/mismatch → rechazo, sin sesión. (HU-001 AC3)
- [x] 1.5 Implementar endpoints `/auth/linkedin/start` (genera `state`) y `/auth/linkedin/callback` (valida `state`, usa `ILinkedInClient`). — `Program.cs`.
- [x] 1.6 (test-first) Test: primer acceso crea `users` con `User_ID` nuevo. (HU-002 AC1) — `AccountProvisioningServiceTests` + el smoke consulta la fila real en `users` vía `psql`.
- [x] 1.7 (test-first) Test: fallo al persistir → sin sesión, sin cuenta parcial. (HU-002 AC2) — dos niveles: la reacción de la orquestación (sin sesión + mensaje) con `FailingUserRepository` —sin contador, porque un `Count` sobre un doble sería vacuo—, y "sin cuenta parcial" contra Postgres real en `PostgresUserRepositoryTests.Fallo_durante_el_alta_no_emite_sesion_ni_deja_cuenta`. **Nota:** no hay transacción explícita ni rollback; la atomicidad la da que el alta es un único statement (`INSERT ... ON CONFLICT; SELECT`) y el fallo se aísla en la orquestación. Si EP-002 añade escrituras al alta, habrá que introducir la transacción de verdad.
- [x] 1.8 (test-first) Test: acceso recurrente (mismo `linkedin_sub`) reutiliza `User_ID`, no duplica. (HU-002 AC3) — smoke: 2º login = mismo userId **y** `select count(*)` sobre `users` = 1 (comprobación repetible, no manual).
- [x] 1.9 Implementar provisión idempotente por `linkedin_sub` (`ON CONFLICT`) + emisión de JWT (`JwtIssuer` HS256). — `PostgresUserRepository`.
- [x] 1.10 journey_smoke SS-a: `docker compose up` + flujo `/start`→`/callback` mock retorna JWT y persiste `users` (verificado con curl + psql). `EP-001-a.status=done`.

## SS-001-b · Middleware de sesión + endpoints protegidos (HU-003) — ✅ DONE

- [x] 2.1 (test-first) Test: petición con token válido → middleware resuelve `User_ID`, 200 con datos. (HU-003 AC1) — `SessionMiddlewareTests` (WebApplicationFactory).
- [x] 2.2 (test-first) Test: token ausente/manipulado → 401, sin operación. (HU-003 AC2)
- [x] 2.3 (test-first) Test: token expirado → 401. (HU-003 AC3) — `ClockSkew=Zero` (sin la tolerancia de 5 min por defecto).
- [x] 2.4 Implementar middleware de validación JWT + endpoint protegido de prueba (`/api/me` retorna `User_ID`). — `AddJwtBearer` + `RequireAuthorization` en `Program.cs`.
- [x] 2.5 journey_smoke SS-b: llamada autenticada 200 / no autenticada 401 en el stack levantado. — smoke: login→JWT→`/api/me` 200 `{userId}`; sin token / manipulado → 401.

## SS-001-c · Login React + cableado end-to-end (HU-001 UI) — ✅ DONE

- [x] 3.1 Componente de login (botón "Continuar con LinkedIn") → redirige a `/auth/linkedin/start`. — `pages/Login.tsx`; el `state` CSRF lo genera el backend (nunca el cliente).
- [x] 3.2 Manejo del callback en el SPA + almacenamiento de sesión + redirección al área autenticada. — `pages/AuthCallback.tsx` (token por fragmento `#token`, `sessionStorage`, hash limpiado del historial) + `auth/RequireAuth.tsx`.
- [x] 3.3 Cableado real al backend (SS-a + SS-b corriendo); llamada protegida `/api/me` con la sesión. — interceptor Bearer en `api/client.ts`; `pages/Perfil.tsx` muestra el `User_ID` resuelto.
- [x] 3.4 Fidelidad: screenshot de la pantalla de login vs `docs/07-prototipo/` vía MCP de navegador (gate `fidelity`). — **FIEL**; evidencia en `docs/.evidencia/EP-001/`.
- [x] 3.5 journey_smoke SS-c end-to-end: clic login → flujo OAuth2 (mock) → área autenticada → `/api/me` 200. — verificado con clic real; además logout, guard de ruta y mensaje de error AC2 en UI.

> Bug encontrado por el journey real (no por inspección): el efecto del callback no era
> idempotente — consumía el fragmento y una segunda ejecución (StrictMode/remount)
> expulsaba al login. Corregido con respaldo en la sesión ya guardada.
> El `docker-compose` monta `frontend/src` para que el HMR recoja cambios sin rebuild.

## SS-001-d · Cerrar sesión (HU-021) — ✅ DONE

> Añadida tras la auditoría: el requisito "Cierre de sesión" de la spec no tenía
> ninguna task, y esa rotura top-down es justo el mecanismo por el que se pierden
> cláusulas de los AC (pasó con "registra el intento" en HU-001 AC3).

- [x] 5.1 (test-first) Test: el control de salir descarta la sesión. (HU-021 AC1) — `App.test.tsx`, sobre el botón real del layout. Mutación que lo mata: quitar `clearSession()` del handler (verificada).
- [x] 5.2 (test-first) Test: tras salir se vuelve al login y no se ve el contenido privado. (HU-021 AC1/AC2) — `App.test.tsx`. Mutación: navegar a `/perfil` en vez de `/login` (verificada).
- [x] 5.3 (test-first) Test: el guard impide entrar al área autenticada sin sesión. (HU-021 AC2) — `RequireAuth.test.tsx`. Mutación: desactivar el guard con `if (false)` (verificada).
- [x] 5.4 (test-first) Test: la sesión no sobrevive al cierre de la pestaña. (HU-021 AC3) — `session.test.ts`: se fija el uso de `sessionStorage`; mutación con `localStorage` lo pone en rojo.

## Cierre del slice

- [x] 4.1 Stack-guard: toda dependencia nueva está en `stack-allowlist.json` con justificación. — **Backend** (`allow_dotnet`): Npgsql 8.0.5, System.IdentityModel.Tokens.Jwt 8.1.2, JwtBearer 8.0.11, Mvc.Testing 8.0.11 (solo tests). **Frontend** (`allow`): `@types/react` y `@types/react-dom` (faltaban en el scaffold y `npm run build` fallaba el typecheck), y `vitest`, `jsdom`, `@testing-library/react` (solo desarrollo; sin ellas no había forma de cubrir el bug de idempotencia del callback ni el guard/logout). `@testing-library/jest-dom` se instaló y se retiró: no se importaba en ninguna parte.
- [x] 4.2 Secreto: verificar que el client secret / JWT key nunca llegan al bundle del frontend (WC-006). — verificado por ejecución: sin coincidencias de secretos en `frontend/src`; la única variable expuesta al cliente es `VITE_API_URL` (una URL). El flujo mantiene el `state` y el secret server-side.
- [x] 4.5 Redes de calidad permanentes (tras la 5ª auditoría): E2E con Playwright contra el artefacto de producción (6 journeys), mutation testing sin exclusiones (backend 53,89 % · frontend 74,07 %), CI que corre las cuatro redes, y modo mock/real explícito con `docker-compose.mock.yml`. La lógica de decisión del flujo salió de `Program.cs` a `LinkedInOAuthOptions` para que sea verificable.
- [ ] 4.3 wiring_verified: verificación adversarial independiente (contexto virgen) de los items del `wiring_checklist`. — EN CURSO
- [ ] 4.4 DoD (reducido, inner loop) verde → PR a `main` + archivar el change en el mismo PR.

## Auditoría de coherencia independiente — hallazgos y resolución

Una auditoría independiente (contexto virgen) declaró el slice **INCOHERENTE** y encontró
huecos reales que el equipo de construcción no había visto. Resolución:

| # | Hallazgo | Resolución |
|---|---|---|
| H1 | HU-001 AC3 exige "y **registra el intento**"; no había logging. La cláusula se perdió al redactar la task 1.4, así que el TDD nunca la forzó | **Corregido con TDD**: `ILogger` en `LinkedInAuthenticationService`; `LogWarning` en el rechazo CSRF. Dos tests: que se registra, y que **no** se vuelca el `state` recibido |
| H2 | El test de HU-002 AC2 no assertaba "sin cuenta parcial" ni el mensaje (prometía más que comprobaba) | **Corregido en dos pasos**: el primer intento (un `Count` en el doble) resultó VACUO — lo demostró la 2ª auditoría por mutación, porque en un doble es el propio doble quien decide si escribe. La verificación vive ahora en `PostgresUserRepositoryTests` contra Postgres real, con el fallo inyectado por un trigger dentro de la misma base |
| H3 | Las tasks 1.6/1.8 y el `wiring_checklist` citaban "verificado en smoke (fila real en users)", pero el smoke era solo `curl` | **Corregido**: el smoke consulta `users` con `psql`. La afirmación ahora es cierta y repetible |
| H4 | Se hablaba de "rollback" sin transacción en el código | **Corregido el texto** (1.7): la atomicidad la da el statement único `ON CONFLICT` |
| H5 | El test de orden CSRF probaba que no se provisionó, no que no se llamara a LinkedIn | **Corregido**: `FakeLinkedInClient.ExchangeCount` + test que asserta 0 canjes con `state` inválido |
| H6 | Logout implementado sin HU que lo pida | **No se retira** (retirar = recortar, decisión del equipo). Trazaba al prototipo aprobado (`docs/07-prototipo/index.html:255`) pero sin historia. **RESUELTO**: se creó `HU-021 — Cerrar sesión` (con AC, spec, tasks 5.1-5.4 y tests de componente) |
| H7 | 17 de 19 checks del smoke corrían sobre `?format=json`; la rama de navegador de los errores no tenía regresión | **Corregido**: 6 checks nuevos sobre los redirects de error reales (`/login?error=...`), incluido que ninguno lleva sesión |
| H8 | CORS `AllowAnyOrigin` sin condicional pese al comentario "solo desarrollo" | **Corregido**: permisivo solo en `IsDevelopment()`; fuera de ahí se restringe a `FRONTEND_URL` |
| H9 | `RealLinkedInClient` es la única ruta de producción y no la ejercita ningún test | **Aceptado y declarado** (WC-002): sin credenciales no hay forma. No se presentará como verificado en el Release Gate |
| H10 | El bug de idempotencia del callback se corrigió sin test de regresión; cero pruebas de frontend | **Corregido**: vitest + jsdom; `resolveSessionToken` extraída y cubierta con 11 tests, 4 de ellos sobre el escenario exacto que falló |

Además, la auditoría destapó un defecto **preexistente del scaffold**: faltaban `@types/react`
y `@types/react-dom`, por lo que `npm run build` (`tsc -b && vite build`) fallaba el typecheck.
Corregido; `tsc -b` y `npm run build` salen con código 0.

## Gates de verificación (fase 5)

- [x] `api` — 22 requests / 38 aserciones sin desviaciones (agente) + `tests/smoke/ep-001-api-contract.sh` **27/27** repetible, sin dependencias nuevas.
- [x] `data` — invariantes verificadas con SQL real; idempotencia cubre la carrera check-then-act vía `ON CONFLICT`. Desviación D1 (guard de `sub` vacío) **corregida** en `LinkedInProfile` con TDD.
