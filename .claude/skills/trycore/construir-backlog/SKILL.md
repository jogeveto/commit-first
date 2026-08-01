---
name: trycore-construir-backlog
description: Construye o consolida `docs/03-backlog/backlog.md` como una tabla ordenada de todas las historias del proyecto, leyendo `docs/04-historias/HU-*.md`. El orden de filas representa la priorización vigente. Path-scope principal: escribe en `docs/03-backlog/backlog.md`, lee desde `docs/04-historias/`. Fase del pipeline: 5 de 7 (obligatoria).
category: Discovery
tags: [backlog, consolidacion, trazabilidad, trycore]
---

# trycore-construir-backlog

Lee `docs/04-historias/HU-*.md` y produce `docs/03-backlog/backlog.md` con una tabla ordenada por prioridad.

## Contexto (fuente: METODOLOGIA.md §6)

> El backlog es una lista **ordenada** de todo lo que se necesita realizar. Sin orden no hay backlog, hay una lista.

## Inputs requeridos

- (Implícitos) Historias en `docs/04-historias/`.
- (Opcional) Si ya existe `backlog.md` previo, preservar el orden previo y solo agregar nuevas historias al final con prioridad provisional.

## Plantilla

`templates/artefactos/backlog.template.md`.

## Reglas duras

1. **Toda historia con `estado: lista` o `en-curso` o `hecha` debe aparecer en el backlog**. Las `draft` aparecen pero claramente marcadas.
2. **Las columnas obligatorias**: `#, ID, Título, Épica, Prioridad, Complejidad, Estado, AC count, Notas`.
3. **El orden de filas ES la priorización vigente**. La skill no inventa priorización — la skill `trycore-priorizar-backlog` se encarga de eso. Si no hay priorización aún, ordenar por ID descendente (o como el usuario indique).
4. **Trazabilidad rápida** al final: épica → historias asociadas (extraído del campo `epica` del frontmatter).

## Flujo de la skill

1. **Glob `docs/04-historias/HU-*.md`** y leer frontmatter YAML + contar AC de cada historia.
2. **Detectar backlog previo** (`docs/03-backlog/backlog.md`). Si existe, preservar el orden de las historias ya listadas y agregar las nuevas al final con prioridad "?".
3. **Generar tabla** desde la plantilla.
4. **Generar resumen** (totales por estado) y trazabilidad épica → historias.
5. **Reportar inconsistencias** (historias huérfanas sin épica, épicas sin historias, etc.).

## Handoff

- **Agente revisor**: `trazabilidad-auditor`. Verifica la cadena AC → US → Épica → Objetivo del PRD.
- **Siguiente skill**: `trycore-priorizar-backlog` cuando el usuario quiera aplicar un framework.

## Notas

- El backlog **no se prioriza** aquí. Esta skill solo consolida. Para priorizar, usar `/trycore:priorizar`.
- Si una historia no tiene épica asignada → marcar como "huérfana" y sugerir corrección.
- Si una historia no tiene AC (count = 0) y no está en estado draft → marcar como inconsistencia.
