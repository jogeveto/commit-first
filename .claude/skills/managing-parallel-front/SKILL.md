---
name: managing-parallel-front
description: Use when building multiple NON-foundational, file-disjoint epics in parallel via git worktrees. Coordinates the parallel_front in build-state.json (selection, worktrees, deterministic merge order with re-smoke). Never parallelizes foundational epics or overlapping file scopes.
---

# Gestionar el front paralelo inter-épica (outer-loop)

**Invariante:** el paralelismo es outer-loop. Cada worktree es un checkout aislado con su
**propio** `build-state.json` y su `active_slice` singular (el inner loop no cambia).

## Precondiciones (compuertas)
- `scaffold.confirmed == true`.
- **G1 Fundacionales primero:** ninguna épica `layer=foundational` abierta. Si aparece una,
  poner `parallel_front.status="draining"` (terminar en curso, no admitir nuevas) antes de abrirla.

## Procedimiento
1. **Reunir candidatas** no fundacionales listas (DoR pasado), cada una con `layer` y `files_scope`.
2. **Seleccionar el conjunto disjunto** (G2):
   `echo "$CANDS" | python3 .claude/scripts/lib/front-plan.py`
   → `selected` van al front; `serialized` esperan (construir secuencial después);
   `excluded_foundational` nunca en paralelo.
3. **Abrir un worktree por épica seleccionada:**
   `git worktree add ".wt/<epica>" -b feature/<slug>` (rama por `1 épica = 1 rama = 1 PR`).
   Registrar el miembro en `parallel_front.members[]` (`merge_status:"pending"`).
4. **Construir cada worktree** con la skill `building-a-slice` (inner loop normal, en su cwd).
   Cada uno mantiene su `journey_smoke` verde localmente.
5. **Coordinar merge (G3)** en `merge_order` (determinista: por orden de épica):
   - Merge del PR; tras cada merge, **re-smoke del journey completo** en el árbol principal.
   - Conflicto → `merge_status:"conflict"`, serializar la perdedora (rebase + re-correr sus gates).
   - Éxito → `merge_status:"merged"`; `git worktree remove`.
6. **Cerrar el front** cuando todos `merged`: `parallel_front=null`.

## Reglas duras
- Nunca escritores paralelos sobre el mismo árbol (por eso worktrees).
- Nunca paralelizar dentro de una épica (preserva la "regla del esqueleto que camina").
- Un solape de `files_scope` no detectado que cause conflicto de merge → el re-smoke (G3) lo caza.
