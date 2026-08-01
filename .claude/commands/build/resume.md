---
name: "BUILD: Resume"
description: Rehidrata el slice activo desde disco tras un reinicio de contexto (reconcilia estado, muestra continuidad y la siguiente acción).
category: Workflow
tags: [build-harness, resume, rehydrate, trycore]
---

# /build:resume — Retomar sin pérdida

Objetivo: reconstruir el contexto de construcción **desde disco**, no desde la conversación.

## Pasos

1. **Reconciliar**: `python3 .claude/hooks/build/reconcile-build-state.py .claude/state/build-state.json`
   (degrada wiring sin evidencia; anota branch drift; fail-open).

2. **Leer estado**: `.claude/state/build-state.json`. Extraer `active_slice`, sus `gates`, `wiring_checklist`, `progress_log`, `session_continuity`, y `parallel_front` si existe.

3. **Determinar la siguiente acción por prioridad** (la primera que aplique):
   1. `session_continuity.resume_hint` presente → ejecutarla.
   2. Items de `wiring_checklist` en `failing` → cablear el primero (con prueba real; no marcar passing sin evidencia).
   3. `sub_slices` con `status!=done` → construir el siguiente.
   4. Según `active_slice.phase` → continuar el pipeline (delegar en la skill `building-a-slice`).

4. **Si hay `parallel_front`**: delegar en la skill `managing-parallel-front` (Task 12+).

5. Registrar un hito en `progress_log[]` (`by: /build:resume`).

**Regla dura:** mientras quede un item `failing`, el slice NO está terminado. Nunca marques `passing` sin evidencia de ejecución real.
