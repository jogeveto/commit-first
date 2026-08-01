---
name: "TRYCORE: PRD"
description: Redacta el PRD del proyecto con los 12 componentes obligatorios. Soporta modos one-pager, completo o para-agentes.
category: Discovery
tags: [prd, requisitos, producto, trycore]
---

Invoca el skill **trycore-escribir-prd**.

**Input** (opcional, tras el comando): brief de 1-2 frases del producto.

---

## Comportamiento

1. **Preflight**: verificar que existe `docs/01-prd/` (lo crea el install). Si no, error orientando al install.
2. **Detectar contexto**: leer `CLAUDE.md` y auto-memory tipo `project` (nombre, dominio, stakeholders).
3. **Preguntar modo** con AskUserQuestion (3 opciones):
   - **One-pager** (Recommended) — rápido, 4 componentes mínimos, se extiende después.
   - **Completo** — los 12 componentes desde el inicio.
   - **Para agentes** — completo + Anexo A con fases secuenciales.
4. **Recopilar inputs faltantes** (problema, audiencia, objetivos, restricciones) con `AskUserQuestion`.
5. **Invocar skill `trycore-escribir-prd`** que genera el archivo.
6. **Mostrar gaps detectados** y proponer próximo paso.

---

## Output esperado

`docs/01-prd/<slug>.md` con:
- Frontmatter de estado.
- 12 componentes (o los del modo elegido).
- Sección de gaps marcada con `<!-- TODO: ... -->` donde corresponda.

---

## Próximo paso sugerido tras completarse

- Si hay gaps: corregirlos con re-invocación o pasar a `/trycore:revisar`.
- Si está limpio: `/trycore:epicas` para descomponer.
