# Asistente de Empleabilidad IA

Adapta hojas de vida con IA local para superar filtros ATS de portales de empleo. Multi-usuario con aislamiento estricto de datos.

> **Discovery completa** en `docs/` (PRD, épicas, historias con AC, backlog priorizado, flows). Construcción gobernada por `@trycore/spec-build-harness`.

## Arquitectura (container-first)

Todo corre en contenedores; el host **no** necesita .NET SDK, Python ni Node instalados (solo Docker).

| Servicio | Stack | Puerto | Health |
|---|---|---|---|
| `frontend` | React (Vite + TS) | 5173 | UI |
| `backend` | .NET Core (minimal API) | 8080 | `/health` |
| `ats-service` | Python (FastAPI, auditor ATS) | 8000 | `/health` |
| `db` | PostgreSQL 16 | 5432 | `pg_isready` |

## Levantar el esqueleto

```bash
cp .env.example .env      # ajusta secretos
docker compose up --build
```

Verificación:
- Backend:  http://localhost:8080/health  → `{ "status": "ok", "service": "backend-api" }`
- ATS:      http://localhost:8000/health  → `{ "status": "ok", "service": "ats-service" }`
- Frontend: http://localhost:5173         → SPA con navegación del sitemap y estado de la API
- DB:       PostgreSQL en `localhost:5432` (esquema base multi-tenant aplicado)

## Estado

Este es el **scaffold** (Paso 1 del build harness): estructura + wiring end-to-end, **sin lógica de negocio**. Las capacidades se construyen por épica (EP-001…EP-006) con el inner loop del arnés (DoR → OpenSpec change → TDD → smoke → DoD → PR). Orden: cimiento (EP-001 auth, EP-002 multi-tenant) antes que negocio.

**Pendiente antes de la UI de negocio**: prototipo clickeable del 100% de la solución (`docs/07-prototipo/`), fuente de verdad del gate de fidelidad visual.
