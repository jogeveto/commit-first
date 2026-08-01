---
name: "TRYCORE: Onboard"
description: Parametriza un proyecto Trycore recién instalado — nombre, dominio, stakeholders, framework de priorización. Puebla el bloque marcado de CLAUDE.md y guarda en auto-memory.
category: Workflow
tags: [onboarding, parametrizacion, trycore]
---

Parametriza un proyecto recién instalado con `trycore-spec init`. **No** crea estructura física (eso ya lo hizo el CLI) — solo recopila valores y los registra.

---

## Preflight

```bash
test -f .claude/.trycore-version || echo "NOT_INSTALLED"
```

**Si no instalado:**

> Este proyecto no tiene la vertical Trycore instalada. Ejecuta primero:
> ```
> npm install -g @trycore/spec-product-flow   # si aún no tienes el CLI
> trycore-spec init                            # en este directorio
> ```
> Luego vuelve a `/trycore:onboard`.

Stop aquí si no está instalado.

---

## Fase 1: Bienvenida

```
## Onboarding Trycore

Voy a parametrizar este proyecto con la vertical documentaria de Trycore. Tarda 2-3 minutos.

Vas a contarme:
1. Nombre del proyecto
2. Dominio (1 línea)
3. Stakeholders principales
4. Framework de priorización por defecto

Después de esto:
- El bloque marcado en CLAUDE.md queda lleno con tus valores
- La auto-memory (tipo `project`) recuerda el framework y stakeholders
- Estás listo para `/trycore:flujo` o cualquier comando individual
```

---

## Fase 2: Recopilación

Usa **AskUserQuestion** (preguntas no-multiSelect cuando el valor es único):

1. *"¿Cuál es el nombre del proyecto?"* — sin opciones, free text.
2. *"Describe el dominio en 1 línea (ej. 'app de reservas de canchas deportivas')"* — free text.
3. *"¿Stakeholders principales? (rol + nombre o equipo, separados por coma)"* — free text.
4. *"¿Qué framework de priorización vas a usar por defecto?"* — opciones:
   - MoSCoW (Recommended) — simple, intuitivo para stakeholders no-técnicos
   - RICE — cuantitativo, requiere datos
   - Valor / Esfuerzo — matriz 2×2 rápida
   - Eisenhower — urgencia × importancia

---

## Fase 3: Actualizar CLAUDE.md

Lee el archivo `CLAUDE.md` actual del proyecto. Encuentra el bloque entre `<!-- BEGIN trycore-vertical` y `<!-- END trycore-vertical -->`.

Reemplaza dentro del bloque las plantillas `{{PROJECT_NAME}}`, `{{DOMAIN_HINT}}`, `{{STAKEHOLDERS}}`, `{{PRIORITIZATION_FRAMEWORK}}` por los valores recogidos.

**NO toques nada fuera de los markers.**

---

## Fase 4: Guardar en auto-memory

Crear/actualizar memorias de **tipo `project`** (no `feedback` — los valores cambian por proyecto):

- `project_name.md` → nombre del proyecto
- `project_domain.md` → dominio
- `project_stakeholders.md` → lista de stakeholders
- `prioritization_framework.md` → framework elegido

Cada memoria con frontmatter:

```yaml
---
name: <slug>
description: <1 línea de qué contiene>
metadata:
  type: project
---
```

Y agregar entradas a `MEMORY.md`.

---

## Fase 5: Confirmación

```
## ✓ Onboarding completo

Proyecto: <nombre>
Dominio: <dominio>
Stakeholders: <lista>
Framework: <framework>

CLAUDE.md actualizado. Auto-memory parametrizada.

## Próximos pasos

| Comando | Para qué |
|---|---|
| `/trycore:flujo` | Pipeline completo PRD→Backlog priorizado |
| `/trycore:prd` | Solo escribir el PRD |
| `/trycore:revisar` | Cuando ya tengas artefactos, auditar todo |
```

---

## Guardrails

- No avanzar sin las 4 respuestas. Si el usuario omite alguna, repreguntar.
- Si CLAUDE.md no tiene el bloque marcado (caso raro post-install), **regenerarlo** desde `~/trycore-spec-product-flow/templates/CLAUDE.md.template` antes de seguir.
- Si el framework elegido es distinto al que ya está en memory, sobrescribir (los proyectos cambian de framework a veces).
