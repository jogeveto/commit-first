## Why

El sistema necesita una puerta de entrada sin fricción: los usuarios (familia) deben poder acceder sin crear contraseñas ni llenar formularios de registro, y toda petición autenticada debe portar una identidad (`User_ID`) que es la clave del aislamiento multi-tenant del resto del producto. Sin identidad no funciona ninguna otra épica; por eso EP-001 es cimiento y se construye primero.

## What Changes

- **Nuevo** flujo de login **LinkedIn OAuth2** (Authorization Code) en el backend .NET: endpoint de inicio, callback, validación de `state` (CSRF), intercambio de código por perfil vía `ILinkedInClient`.
- **Nuevo** cliente de frontera `ILinkedInClient` con implementación **`FakeLinkedInClient`** (mock-first, determinista para tests) inyectable; la implementación real se activa por configuración (`LINKEDIN_CLIENT_ID/SECRET`) sin tocar el dominio.
- **Nueva** provisión automática de cuenta **idempotente** en PostgreSQL (tabla `users`, clave natural `linkedin_sub`): primer acceso crea `User_ID`; acceso recurrente reutiliza el existente.
- **Nueva** emisión de **sesión/JWT** firmada server-side ligada al `User_ID`, y **middleware** de autorización que resuelve el `User_ID` en el contexto de cada petición protegida (401 ante token ausente/inválido/expirado).
- **Nueva** pantalla de **login React** ("Continuar con LinkedIn") cableada al backend, fiel al prototipo (`docs/07-prototipo/`).

No hay cambios BREAKING (es la primera capacidad del sistema).

## Capabilities

### New Capabilities
- `autenticacion-e-identidad`: autenticación vía LinkedIn OAuth2 sin contraseñas, provisión automática e idempotente de cuenta, y emisión/validación de sesión ligada al `User_ID`. Cubre el login, el alta y el mecanismo de sesión que habilitan el aislamiento multi-tenant.

### Modified Capabilities
<!-- Ninguna: es la primera capacidad; no hay specs previas en openspec/specs/. -->

## Impact

- **Backend** (`backend/`): endpoints `/auth/linkedin/start`, `/auth/linkedin/callback`, un endpoint protegido de prueba; middleware de sesión; `ILinkedInClient` + `FakeLinkedInClient`; capa de acceso a `users`.
- **Datos** (`db/`): uso de la tabla `users` del scaffold (esquema base multi-tenant); provisión idempotente por `linkedin_sub`.
- **Frontend** (`frontend/`): pantalla de login + manejo del callback + almacenamiento de sesión en el cliente.
- **Config/Secretos**: `LINKEDIN_CLIENT_ID`, `LINKEDIN_CLIENT_SECRET`, `LINKEDIN_REDIRECT_URI`, `JWT_SIGNING_KEY` (server-side, nunca en el frontend). Sin credenciales reales aún → mock-first.
- **Dependencias**: potencial `Microsoft.AspNetCore.Authentication.*` / Npgsql / EF Core en el backend (validar contra `.claude/config/stack-allowlist.json` al agregarlas).

## Trazabilidad

- **Épica**: `EP-001` — Autenticación e Identidad (LinkedIn OAuth2) · `layer: foundational`
- **HU cubiertas**: `HU-001` (login OAuth2), `HU-002` (provisión automática de cuenta), `HU-003` (sesión ligada al User_ID)
- **Objetivo del PRD**: `O5` — 100% de altas sin formulario manual (y habilita `O3` aislamiento al emitir el `User_ID`).
- **Sub-slices** (gate de tamaño, ≥3 capas): `EP-001-a` backend OAuth2 + provisión · `EP-001-b` middleware de sesión + endpoints protegidos · `EP-001-c` login React + cableado end-to-end.
- **Flow de referencia**: `docs/06-flows/EP-001-autenticacion-identidad.md`.
- **Fuente de diseño (UI)**: pantalla de login en `docs/07-prototipo/index.html`.
