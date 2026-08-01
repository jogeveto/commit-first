---
name: trycore-revisar-calidad-documental
description: Orquesta a TODOS los agentes revisores sobre los artefactos de `docs/`. Produce un único reporte consolidado en `docs/.reviews/<timestamp>-global.md`. Úsalo on-demand antes de un milestone o entrega al cliente (equivalente manual a tener `TRYCORE_AUTO_AUDIT=true`). Path-scope: lectura global de `docs/`, escribe en `docs/.reviews/`. Activación: ON-DEMAND, no automática — el usuario invoca explícitamente vía `/trycore:revisar` o pidiendo "auditoría completa".
category: Quality
tags: [auditoria, calidad, revision, trycore]
---

# trycore-revisar-calidad-documental

Audita todos los artefactos del proyecto despachando agentes revisores en paralelo. Es el "checkup completo" antes de una entrega.

## Inputs requeridos

- Ninguno obligatorio. Si el usuario apunta a un subconjunto (ej. "solo historias"), restringir.

## Reglas duras

1. **Despachar en paralelo** todos los agentes aplicables al estado del proyecto (skill `dispatching-parallel-agents` si está disponible).
2. **Reporte único** consolidado, no N reportes separados al usuario. Los detalles van a `docs/.reviews/<timestamp>-<agente>.md`.
3. **Ranking de issues** por severidad: bloqueante / mayor / menor / sugerencia.
4. **Acciones concretas** por issue: qué historia/PRD/épica corregir y cómo.

## Flujo de la skill

1. **Inventariar artefactos**: PRD existente, épicas, mapa de historias, historias con/sin AC, backlog, priorización.
2. **Despachar agentes** en paralelo según lo que exista:
   - `prd-reviewer` si hay PRD.
   - `story-decomposer-auditor` si hay épicas.
   - `mapping-coherence-auditor` si hay mapa de historias.
   - `invest-validator` para todas las historias.
   - `bdd-validator` para todas las historias con AC.
   - `trazabilidad-auditor` siempre (es el más caro pero el más valioso).
   - `priorizacion-auditor` si hay priorización vigente.
3. **Recoger reportes** y consolidar.
4. **Escribir** `docs/.reviews/<YYYYMMDD-HHMMSS>-global.md` con:
   - Resumen ejecutivo (cuántos issues por severidad).
   - Lista de issues ordenada por severidad.
   - Acciones recomendadas (top 5).
5. **Resumir al usuario** los 3-5 issues más críticos y proponer ejecutar las skills correspondientes para corregirlos.

## Handoff

- No hay "siguiente skill" automática — el usuario decide qué corregir primero.
- Los hooks de PostToolUse se mantienen como complemento (corren tras cada edit); esta skill es la auditoría completa periódica.

## Frecuencia recomendada

- 1× por sprint review.
- Antes de cualquier entrega formal al cliente.
- Después de cambios mayores en el PRD (re-validar trazabilidad).
