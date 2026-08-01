---
name: trycore-escribir-prd
description: Redacta un PRD (Product Requirements Document) con los 12 componentes obligatorios de la metodología Trycore. Úsalo cuando el usuario pida "crear/redactar un PRD", "documento de requisitos de producto", "PRD optimizado para agentes", o como primer paso del pipeline PRD→Backlog. Path-scope principal: archivos bajo `docs/01-prd/`. Fase del pipeline: 1 de 7 (obligatoria, primera).
category: Discovery
tags: [prd, requisitos, producto, discovery, trycore]
---

# trycore-escribir-prd

Escribe el PRD del proyecto en `docs/01-prd/<slug>.md`. Es el **primer artefacto** del pipeline y la fuente de verdad para todo lo que viene después (épicas, historias, backlog).

## Contexto (fuente: METODOLOGIA.md §1)

Un PRD válido tiene **12 componentes** (10 clásicos + 2 modernos: Non-goals y KPIs). Si se va a consumir por agentes de IA, anexar la versión "fases secuenciales" en el Anexo A.

Variante **One-Pager** permitida en fase temprana — pero la entrega final requiere los 12.

## Inputs requeridos

Antes de escribir, asegúrate de tener (preguntar con `AskUserQuestion` lo que falte):

1. **Nombre del producto** (1 línea).
2. **Problema concreto** que resuelve (1-3 frases).
3. **Audiencia / usuarios primarios** (rol + contexto).
4. **Objetivos** (3-5, medibles).
5. **Restricciones conocidas** (tecnología existente, presupuesto, timeline, regulatorio).
6. **Modo**: `one-pager` (rápido) | `completo` (los 12) | `para-agentes` (con Anexo A de fases).

Lee `CLAUDE.md` del proyecto y la auto-memory tipo `project` para nombre, dominio y stakeholders ya capturados por `/trycore:onboard`.

## Plantilla

`templates/artefactos/prd.template.md` (en la raíz del repo trycore-spec-product-flow).

Cuando el repo está instalado, el path canónico es:
- desde proyecto consumidor: `.claude/skills/trycore/escribir-prd/` → resolver el template vía el symlink.

## Reglas duras (rejection rules)

1. **Faltan componentes** → no se acepta. Los 12 deben aparecer (pueden tener placeholders, pero los headers existen).
2. **Objetivos no medibles** → marcar y forzar reescritura. "Mejorar la experiencia" no es medible; "reducir tiempo de búsqueda de 30s a 10s" sí.
3. **Non-goals vacíos** → no se acepta. Si todo es alcance, no hay alcance.
4. **KPIs sin línea base ni meta** → marcar como gap. La tabla de KPIs requiere mínimo línea base + meta + cuándo se mide.
5. **Modo "para-agentes" sin fases secuenciales** → si el modo lo activa pero falta Anexo A, no entregar.

## Flujo de la skill

1. **Detectar contexto**: leer `CLAUDE.md` y memory `project`. Identificar nombre, dominio, stakeholders.
2. **Recopilar inputs faltantes** con `AskUserQuestion`. Máximo 4 preguntas, multi-select donde aplique.
3. **Generar borrador** desde el template, sustituyendo `{{...}}` por valores reales o por marcadores `<!-- TODO: ... -->` si falta info.
4. **Aplicar reglas duras** internamente: si detectas que algún componente queda obviamente vacío o falso, marcarlo y reportarlo al usuario.
5. **Escribir** `docs/01-prd/<slug>.md` con el slug derivado del nombre del producto.
6. **Resumir al usuario** los gaps detectados y proponer el próximo paso.

## Handoff

- **Agente revisor**: `prd-reviewer`. Si los hooks PostToolUse están activos (`TRYCORE_AUTO_AUDIT=true`), se dispara automáticamente; si no, sugerir al usuario correr `/trycore:revisar` o re-invocar este skill con el reporte.
- **Siguiente skill en el pipeline**: `trycore-descomponer-prd-a-epicas` (o el meta-skill `trycore-flujo-prd-a-backlog` si el usuario está en flujo completo).

## Memoria

- **NO** escribas auto-memory desde esta skill. La parametrización del proyecto (nombre, dominio, stakeholders, framework) ya la hizo `/trycore:onboard` con tipo `project`.
- Si el usuario te da decisiones nuevas durante la redacción (ej. "siempre incluye sección de accesibilidad WCAG"), no las guardes tú — sugiérele añadirlas a `CLAUDE.md` o a la memoria explícitamente.

## Ejemplo de invocación

Usuario: *"Crea el PRD para una app de reservas de canchas deportivas."*

1. Skill lee CLAUDE.md → encuentra `PROJECT_NAME` vacío.
2. AskUserQuestion: nombre exacto, audiencia primaria, 3 objetivos, modo.
3. Genera `docs/01-prd/reservas-canchas-deportivas.md` con los 12 componentes y placeholders donde falta info dura.
4. Reporta: "Generado el PRD con 12 componentes. Gaps detectados: KPIs sin línea base (3), Stakeholders sin nombrar (2). ¿Quieres llenar gaps ahora, o seguir al siguiente paso `/trycore:epicas` y volver después?"

## Variante "PRD optimizado para agentes" (modo para-agentes)

Estructurar el cuerpo en **fases secuenciales** donde cada fase declara:

```markdown
### Fase N: <título>
- **Dependencias**: <Fase N-1 | ninguna>
- **Resultado verificable**: <qué queda hecho al terminar>
- **Alcance**: <qué SÍ; qué NO>
- **Tiempo estimado del agente**: <5-15 min>
```

Esta variante es ideal cuando el PRD será input directo de un agent de coding que ejecuta el plan.
