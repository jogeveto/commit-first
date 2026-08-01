---
name: "TRYCORE: Backlog"
description: Consolida todas las historias en docs/03-backlog/backlog.md como tabla ordenada. NO prioriza (eso es /trycore:priorizar) — solo consolida y reporta inconsistencias.
category: Discovery
tags: [backlog, consolidacion, trazabilidad, trycore]
---

Invoca el skill **trycore-construir-backlog**.

---

## Comportamiento

1. **Glob** `docs/04-historias/HU-*.md`.
2. **Leer frontmatter YAML** de cada historia + contar AC.
3. **Detectar backlog previo**: si existe, preservar orden previo; agregar nuevas al final con prioridad "?".
4. **Generar tabla** con columnas: `#, ID, Título, Épica, Prioridad, Complejidad, Estado, AC count, Notas`.
5. **Generar resumen** (totales por estado).
6. **Generar trazabilidad rápida** épica → historias asociadas.
7. **Reportar inconsistencias**:
   - Historias huérfanas (sin épica).
   - Historias sin AC pero no en draft.
   - Épicas sin historias asociadas.

## Próximo paso

`/trycore:priorizar` para aplicar un framework y ordenar el backlog formalmente.
