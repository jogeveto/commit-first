---
name: "BUILD: Front"
description: Abre y coordina un front paralelo de épicas NO fundacionales y disjuntas en archivos, cada una en su worktree/rama/PR. Delega en la skill managing-parallel-front (selección disjunta vía scripts/lib/front-plan.py, worktrees, merge en orden con re-smoke).
category: Workflow
tags: [build-harness, outer-loop, front-paralelo, trycore]
---

# /build:front — Front paralelo inter-épica

Delega en la skill **managing-parallel-front**. Resumen:
1. Verifica precondiciones (scaffold confirmado; sin épica foundational abierta).
2. Reúne candidatas no fundacionales listas (DoR pasado) con `layer` y `files_scope`.
3. Selecciona el conjunto disjunto (`scripts/lib/front-plan.py`), abre worktrees, construye y mergea en orden con re-smoke.

Úsalo solo cuando haya ≥2 épicas no fundacionales disjuntas listas. Para una sola épica, usa `/build:slice`.
