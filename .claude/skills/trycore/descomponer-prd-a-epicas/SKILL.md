---
name: trycore-descomponer-prd-a-epicas
description: Descompone un PRD existente en épicas trazadas a los objetivos del PRD. Cada épica recibe ID EP-XXX y se agrupa en `docs/03-backlog/epicas.md`. Úsalo después de tener un PRD válido y antes de escribir historias. Path-scope principal: escribe en `docs/03-backlog/epicas.md`, lee desde `docs/01-prd/`. Fase del pipeline: 2 de 7 (obligatoria).
category: Discovery
tags: [epicas, decomposition, backlog, trazabilidad, trycore]
---

# trycore-descomponer-prd-a-epicas

Toma el PRD del proyecto (`docs/01-prd/<slug>.md`) y produce las épicas en `docs/03-backlog/epicas.md`. Cada épica se traza explícitamente a uno o más objetivos del PRD.

## Contexto (fuente: METODOLOGIA.md §2)

Taxonomía: **Roadmap → Épica → Historia de Usuario → Ticket**.

- Una épica es una agrupación grande de trabajo. Se desglosa en múltiples historias.
- Tiene ID propio `EP-XXX` (3 dígitos).
- Es componente del roadmap.

**Regla dura de trazabilidad**: toda épica cubre ≥ 1 objetivo del PRD. Todo objetivo del PRD es cubierto por ≥ 1 épica.

## Inputs requeridos

- PRD existente en `docs/01-prd/` (al menos uno). Si no existe, redirigir al usuario a `/trycore:prd`.

## Plantilla

`templates/artefactos/epicas.template.md`.

## Reglas duras

1. **Cobertura bidireccional**: cada épica → ≥1 objetivo; cada objetivo → ≥1 épica. Reportar huérfanos.
2. **Granularidad coherente**: las épicas deben ser de tamaño comparable. Si una épica es 10× más grande que otra, dividirla.
3. **Sin solapes**: dos épicas no deben cubrir la misma capability. Si lo hacen, refactorizar.
4. **IDs únicos**: `EP-001`, `EP-002`, … No saltar números, no reusar.
5. **Métrica de éxito por épica**: cada épica declara algo medible cuando esté completa (puede heredar del KPI del PRD).

## Flujo de la skill

1. **Leer PRD** del proyecto. Extraer objetivos (sección 1), capabilities (sección 5) y KPIs (sección 12).
2. **Identificar agrupaciones naturales**: las capabilities del PRD suelen mapear casi 1:1 a épicas; los objetivos suelen ser transversales y pueden requerir épicas que los crucen.
3. **Generar épicas** con IDs correlativos. Cada una con: título, resumen, justificación, objetivos del PRD que cubre, capabilities, métrica de éxito.
4. **Validar trazabilidad** internamente: matriz Épica × Objetivo-PRD.
5. **Escribir** `docs/03-backlog/epicas.md`.
6. **Reportar gaps** (objetivos sin épica que los cubra, épicas sin objetivo claro).

## Handoff

- **Agente revisor**: `story-decomposer-auditor`. Confirma cobertura bidireccional y granularidad.
- **Siguiente skill**: `trycore-crear-mapa-historias` (para visualizar el journey) o `trycore-escribir-historia-usuario` (para entrar directo al detalle).

## Ejemplo

PRD con 4 objetivos (O1, O2, O3, O4) y 6 capabilities (C1..C6) → output típico:

```
EP-001 — Búsqueda y descubrimiento       → cubre O1, O2 (capabilities C1, C2)
EP-002 — Gestión de reservas             → cubre O3 (capabilities C3, C4)
EP-003 — Pagos y facturación             → cubre O3, O4 (capabilities C5)
EP-004 — Administración y reportería     → cubre O4 (capabilities C6)
```

Matriz de trazabilidad al final del archivo.
