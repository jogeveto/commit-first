---
name: "BUILD: Onboard"
description: Parametriza el dominio del build harness — capa de servicios externos/IA, lógica determinista, PII, secretos, decisiones de alto impacto y fuente de diseño (DESIGN_SOURCE). Rellena el bloque marcado de CLAUDE.md y escribe auto-memory. Complementa al CLI trycore-build init (que ya sembró los archivos y el stack mecánico).
category: Workflow
tags: [onboarding, parametrizacion, build-harness, trycore]
---

Parametriza el **dominio** de un proyecto donde ya se instaló el arnés con `trycore-build init`. El CLI ya sembró los archivos, el `stack-allowlist.json` y el bloque marcado de CLAUDE.md con `{{placeholders}}`. Tu trabajo es resolver esos placeholders (que requieren leer e interpretar el PRD) y escribir la memoria — **cosas que el binario Node no puede hacer**.

---

## Preflight

```bash
test -f .claude/.build-harness-version || echo "NOT_INSTALLED"
```

**Si no instalado:**

> Este proyecto no tiene el arnés de construcción instalado. Ejecuta primero:
> ```
> npm install -g @trycore/spec-build-harness   # si aún no tienes el CLI
> trycore-build init                            # en este directorio
> ```
> Luego vuelve a `/build:onboard`.

Stop aquí si no está instalado.

---

## Fase 1: Bienvenida

```
## Onboarding del arnés de construcción

El CLI ya instaló agentes, comandos /opsx:*, hooks y el estado. Ahora voy a parametrizar el
DOMINIO del arnés (2-3 min) — los puntos de extensión que leen los agentes de calidad
(security-reviewer, stack-guardian, data-consistency-checker, ux-krug-reviewer, simple-design-reviewer, ux-fidelity-reviewer).

Voy a leer tu PRD/openspec para proponer valores y confirmar contigo:
1. Ruta#ancla del PRD técnico (fuente del stack)
2. Capa de servicios externos / IA (la frontera)
3. Lógica que debe ser determinista (no delegable a IA)
4. Categorías de datos sensibles / PII reguladas
5. Secretos server-side
6. Decisiones de alto impacto que exigen explicabilidad en UX
7. Fuente de diseño / referencia visual (prototipo/export) y pantallas — o "N/A" si no hay UI
8. Capa de cada épica del backlog: **fundacional** (cimiento) vs **negocio**
```

---

## Fase 2: Leer el PRD y recopilar

1. Localiza y lee el PRD técnico del consumidor y/o `openspec/project.md` si existen
   (típico: `docs/01-prd/*.md`, sección de requisitos técnicos). Si no hay PRD, opera solo con
   las respuestas del usuario.
2. **Propón** valores derivados del PRD y confírmalos vía **AskUserQuestion** (un ítem por punto;
   ofrece tu propuesta como primera opción "(Recomendado)"):
   - **PRD técnico** (`PRD_TECH_PATH`): ruta#ancla de la sección de requisitos técnicos.
   - **Capa de servicios externos / IA** (`EXTERNAL_SERVICE_LAYER`): ¿hay un servicio externo/IA?, ¿cuál es su frontera/aislamiento? (ej. "capa de extracción documental").
   - **Lógica determinista** (`DETERMINISTIC_LAYER`): ¿qué lógica NO puede delegarse a un servicio no determinista? (ej. "motor de decisión/reglas").
   - **Datos sensibles / PII** (`SENSITIVE_DATA_CATEGORIES`): categorías reguladas del dominio.
   - **Secretos server-side** (`SERVER_SIDE_SECRETS`): claves/tokens que jamás van al cliente.
   - **Decisiones de alto impacto** (`HIGH_STAKES_DECISIONS`): decisiones que exigen explicabilidad/justificación en la UI.
   - **Fuente de diseño** (`DESIGN_SOURCE`): ¿el producto tiene UI? Si sí, ruta/URL de la fuente
     visual de verdad (prototipo, export de diseño o mockups) y cómo localizar cada pantalla; si no,
     "N/A". (La escritura del estado `design_source` en `build-state.json` se hace en la Fase 3c.)
3. Si un punto no aplica al proyecto, registra explícitamente "no aplica" (no lo dejes como `{{...}}`).

---

## Fase 2b: Clasificar la capa de las épicas (cimiento vs negocio)

El factor que más reduce el consumo de contexto por slice es que el **cimiento** ya esté construido y
abstraído antes de que el loop tome historias de negocio. Para habilitar el gate de DoR "Cimiento
construido":
1. Lee el backlog/Story Map del proyecto (`docs/03-backlog/epicas.md`, `docs/02-user-story-map/`).
2. Propón, vía **AskUserQuestion**, qué épicas son **`layer: foundational`** (autenticación, acceso a
   datos, arquitectura base, design-system/componentes base del prototipo) y cuáles **`layer: business`**.
3. Escribe el tag en el **frontmatter de cada épica** en `docs/03-backlog/epicas.md` (artefacto de
   discovery; coordina con `@trycore/spec-product-flow` si ese paquete ya lo gobierna — el build-harness
   solo necesita poder **leer** `layer`). El DoR rechazará abrir una épica de negocio que arrastre
   cimiento `foundational` aún no archivado.

No inventes la clasificación: derívala del PRD/Story Map y confírmala con el usuario.

---

## Fase 3: Resolver el bloque de CLAUDE.md

Lee `CLAUDE.md`. Encuentra el bloque entre `<!-- BEGIN trycore-build-harness` y `<!-- END trycore-build-harness -->`.

**Guardarraíl de markers (antes de escribir):** verifica que exista **exactamente un** par
`BEGIN`/`END trycore-build-harness`:

```bash
b=$(grep -c 'BEGIN trycore-build-harness' CLAUDE.md 2>/dev/null || echo 0)
e=$(grep -c 'END trycore-build-harness' CLAUDE.md 2>/dev/null || echo 0)
[ "$b" = 1 ] && [ "$e" = 1 ] || echo "MARKERS_BAD ($b BEGIN / $e END)"
```

Si imprime `MARKERS_BAD` (0 ó >1 pares) → **STOP**: pide correr `trycore-build update` y **no** edites el bloque.

Reemplaza dentro del bloque los placeholders `{{PRD_TECH_PATH}}`, `{{EXTERNAL_SERVICE_LAYER}}`,
`{{DETERMINISTIC_LAYER}}`, `{{SENSITIVE_DATA_CATEGORIES}}`, `{{SERVER_SIDE_SECRETS}}`,
`{{HIGH_STAKES_DECISIONS}}`, `{{DESIGN_SOURCE}}` por los valores confirmados.

**NO toques nada fuera de los markers.**

---

## Fase 3b: (Opcional) Poblar el stack-allowlist

Si el usuario lo desea y existe `package.json` en el proyecto:
- Lee las dependencias presentes y propón cuáles entran al `allow[]` de
  `.claude/config/stack-allowlist.json` (contrástalo con el PRD técnico).
- Setea `source` a la ruta del PRD técnico (`PRD_TECH_PATH`).
- No incluyas dependencias que el PRD no justifique.

---

## Fase 3c: (Si hay UI) Confirmar la fuente de diseño en el estado

Espejo de la confirmación de scaffold, para `design_source` en `build-state.json`:
- Si el producto **tiene UI**: setea `design_source.applies=true`, `source` (el puntero confirmado) y
  `confirmed=true` **solo si** el usuario confirma que la fuente de diseño existe (con `confirmed_by`,
  `confirmed_at`). El arnés **no genera** el prototipo.
- Si **no hay UI**: setea `design_source.applies=false` (el mecanismo de fidelidad queda N/A).

Escribe **solo** los campos del schema (`applies`, `source`, `confirmed`, `confirmed_by`, `confirmed_at`,
`notes`; el objeto es `additionalProperties:false`) y **valida contra `build-state.schema.json` tras escribir**
(aborta si no valida). Guardarraíl: **una transición = una escritura**; no toques otros campos del estado.

---

## Fase 4: Guardar en auto-memory

Crear/actualizar memorias **tipo `project`** (los valores cambian por proyecto):

- `build_prd_tech_path.md` → ruta#ancla del PRD técnico
- `build_external_service_layer.md` → capa de servicios externos/IA
- `build_deterministic_layer.md` → lógica determinista
- `build_sensitive_data.md` → categorías PII/datos regulados
- `build_server_side_secrets.md` → secretos server-side
- `build_high_stakes_decisions.md` → decisiones de alto impacto
- `build_design_source.md` → fuente de diseño / referencia visual

Cada memoria con frontmatter `type: project`. Agrega entradas a `MEMORY.md`.

---

## Fase 5: Confirmación

```
## ✓ Dominio del arnés parametrizado

PRD técnico:        <ruta#ancla>
Servicios externos: <...>
Determinista:       <...>
PII/datos:          <...>
Secretos:           <...>
Decisiones clave:   <...>
Fuente diseño:      <...>

CLAUDE.md actualizado. Auto-memory escrita. Los agentes de calidad ya leen tu dominio.

## Próximos pasos

| Acción | Para qué |
|---|---|
| `/build:work <descripción>` | Router: enruta a micro-change / slice / release según el cambio |
| `/build:slice [EP-XXX]` | Abrir un slice (épica EP-XXX) e iniciar el inner loop |
| `/build:release [release]` | Correr el Release Gate (outer loop) sobre el diff acumulado |
| `/opsx:new` | Crear un OpenSpec change |
| `trycore-build doctor` | Verificar openspec/python3/hooks |
```

---

## Guardrails

- No avances sin confirmar los 7 puntos. Si el usuario omite alguno, repregunta o marca "no aplica".
- Si CLAUDE.md no tiene el bloque marcado (caso raro post-install), pide correr `trycore-build init` (o `update`) antes de seguir.
- No inventes valores de dominio que no estén en el PRD ni confirmados por el usuario.
- Si un valor ya existe en memory y cambió, sobrescríbelo (los proyectos evolucionan).
- Esto es parametrización **semántica**: NO modifiques agentes/skills/hooks del paquete.
