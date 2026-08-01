---
name: "TRYCORE: Revisar"
description: Auditoría global on-demand. Corre todos los agentes revisores sobre los artefactos de docs/ en paralelo y consolida en docs/.reviews/<timestamp>-global.md. Es el equivalente manual a tener TRYCORE_AUTO_AUDIT=true.
category: Quality
tags: [auditoria, revision, calidad, trycore]
---

Invoca el skill **trycore-revisar-calidad-documental**.

**Input** (opcional): subset a revisar (`prd`, `historias`, `backlog`, `priorizacion`, `trazabilidad`). Sin argumento → revisa todo.

---

## Comportamiento

1. Inventariar `docs/`: qué artefactos existen.
2. Despachar agentes revisores aplicables **en paralelo** (vía `dispatching-parallel-agents` si está disponible):
   - `prd-reviewer` ← si hay PRD
   - `story-decomposer-auditor` ← si hay épicas
   - `mapping-coherence-auditor` ← si hay mapa
   - `invest-validator` ← para todas las historias
   - `bdd-validator` ← para historias con AC
   - `trazabilidad-auditor` ← siempre (Opus)
   - `priorizacion-auditor` ← si hay priorización
3. **Recoger reportes individuales** en `docs/.reviews/<ts>-<agente>.md`.
4. **Consolidar** en `docs/.reviews/<ts>-global.md` con:
   - Resumen ejecutivo (cuántos issues por severidad).
   - Top 5 issues bloqueantes.
   - Top 5 issues mayores.
   - Acciones recomendadas.
5. **Mostrar al usuario** el resumen + top issues; los detalles quedan en el archivo.

---

## Frecuencia recomendada

- 1× por sprint review (típicamente cada 1-2 semanas).
- Antes de cualquier entrega al cliente.
- Después de cambios mayores al PRD (re-validar trazabilidad).

---

## Guardrails

- No bloquear la conversación: los agentes corren en background; mientras tanto, el comando puede mostrar "auditoría en curso" y avanzar a otra cosa si el usuario lo prefiere.
- Si no hay nada que auditar (proyecto recién instalado, `docs/` vacío), reportar y sugerir `/trycore:flujo` o `/trycore:prd`.
