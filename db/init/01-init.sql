-- Esquema base — Asistente de Empleabilidad IA (scaffold, EP-002 cimiento multi-tenant).
-- Solo la base del patrón de tenancy: tabla de usuarios + convención user_id.
-- Las tablas de dominio (perfiles, vacantes, cvs, matches, prompts) se crean por
-- slice con migraciones versionadas; TODAS deben llevar user_id NOT NULL + FK.

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Usuarios: identidad provista por LinkedIn OAuth2 (sin contraseñas).
CREATE TABLE IF NOT EXISTS users (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    linkedin_sub  TEXT NOT NULL UNIQUE,           -- subject estable de LinkedIn (idempotencia del alta)
    display_name  TEXT,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Ejemplo del patrón multi-tenant que TODA tabla de dominio debe seguir.
-- (placeholder de scaffold; se reemplaza/expande en los slices de negocio)
CREATE TABLE IF NOT EXISTS tenant_probe (
    id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id  UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    note     TEXT
);
CREATE INDEX IF NOT EXISTS idx_tenant_probe_user_id ON tenant_probe(user_id);

-- Nota: el filtrado por user_id (HU-005) y la guarda de aislamiento (HU-006)
-- se implementan en la capa de datos del backend; considerar RLS de PostgreSQL
-- como defensa en profundidad en el slice de EP-002.
