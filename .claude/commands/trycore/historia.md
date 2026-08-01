---
name: "TRYCORE: Historia"
description: Escribe una historia de usuario en formato "Como/Quiero/Para" con frontmatter YAML obligatorio. Auto-asigna ID HU-XXX. NO escribe AC (eso es /trycore:ac).
category: Discovery
tags: [user-story, historia-usuario, trycore]
---

Invoca el skill **trycore-escribir-historia-usuario**.

**Input** (opcional): descripción breve de la historia o ID de la épica madre.

---

## Comportamiento

1. **Determinar siguiente ID** auto-incrementando desde `docs/04-historias/HU-*.md`.
2. **Recopilar inputs** (con AskUserQuestion):
   - Épica madre (`EP-XXX`).
   - Rol (específico, no genérico).
   - Acción.
   - Beneficio (externo y visible).
   - Prioridad y complejidad iniciales.
3. **Validar formato** del trio rol/acción/beneficio.
4. **Generar archivo** `docs/04-historias/HU-XXX-<slug>.md` desde la plantilla.
5. **Auto-check INVEST básico**: marcar V, S, T (lo que la skill puede verificar) y dejar I, N, E al equipo.
6. Reportar ID asignado.

## Próximo paso

`/trycore:ac` sobre esta misma historia para escribir los AC.
