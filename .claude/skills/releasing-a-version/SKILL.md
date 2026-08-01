---
name: releasing-a-version
description: Use when closing a release of the product — runs the heavy review gates ONCE over the accumulated diff of a release line (security, design/smell, UX/Krug, three-way coherence, architecture, and full end-to-end integration with real deps), instead of per epic. This is the outer loop; the per-epic inner loop lives in building-a-slice. Trigger after archiving an epic when the user accepts the Release Gate, or when a Story Map release line is complete. Records results in build-state.json releases[].
---

# Release Gate (outer loop) — Build

El **outer loop** del arnés. Mientras `building-a-slice` (inner loop) construye épica a épica de
forma barata y rápida, esta skill ejecuta las **revisiones profundas una sola vez por release**,
sobre el **diff acumulado** de todas las épicas de esa release. Así el costo de los agentes pesados
pasa de `O(épicas)` a `O(releases)`.

## Qué es una release
Una **línea de release del Story Map** (`docs/02-user-story-map/`). P.ej. **R1-mvp** = EP-001 +
EP-002 + EP-003 + EP-004 ("Resolver un caso de punta a punta con resultado explicable"). El conjunto
de épicas de la release son las que vas a auditar en bloque.

## Cuándo se invoca
- Tras archivar una épica, `building-a-slice` pregunta con un default computado y el usuario acepta.
- O explícitamente: "corre el Release Gate de R1".

## Principio de operación
- **Una sola fuente de verdad**: `.claude/state/build-state.json` → array `releases[]`. Lee antes,
  escribe una entrada por release.
- **Sobre el diff acumulado**: el alcance es el rango de commits de todas las épicas de la release
  (desde el merge anterior a la primera épica de la release hasta `main`).
- **Delega en subagentes** (devuelven síntesis, protegen el contexto).

## Workflows (plantillas, no scripts)

Esta skill es el **hogar primario** de los workflows del arnés: el Release Gate corre **una vez por release**
(`O(releases)`), fuera del camino caliente del inner loop, y **no** duplica el inner loop (ni TDD ni gates por
slice). Los `*.workflow.js` bajo `workflows/` son **plantillas de referencia** (no scripts a correr verbatim;
si contradicen `METODOLOGIA.md`, gana la metodología). Reglas duras:
- **Read-only sobre el estado.** La plantilla devuelve veredictos; **esta skill** es la única que escribe
  `releases[]` (una entrada por release, validando contra `build-state.schema.json`, `updated_by:
  releasing-a-version`). Parciales **no** promueven a `passed`.
- **`integration` fuera del paralelo.** Los 5 reviewers pesados van en `parallel()`; el gate `integration`
  (journey completo con **deps reales**) es **secuencial** vía `verify`/`run` y **no** delega en un reviewer.

Ver `workflows/README.md`. Hoy: `workflows/release-gate.workflow.js`.

## Pipeline del Release Gate

| Gate | Acción | Delega en | Referencia |
|---|---|---|---|
| `security` | Vectores generales + foco de dominio del consumidor (PII/secretos/authz, claves de servicios externos server-side) sobre todo el diff | `security-reviewer` | — |
| `smell` | 4 reglas de Beck + code smells sobre el diff acumulado | `simple-design-reviewer` | `building-a-slice/references/simple-design.md` |
| `ux` | Krug + lighthouse sobre la UI ensamblada de la release (o `null` si sin UI) | `ux-krug-reviewer` | `building-a-slice/references/krug-ux.md` |
| `coherence` | Trazabilidad triple AC↔change↔código de **todas** las HU de la release | `coherence-three-way` | — |
| `stack_arch` | Arquitectura del PRD del consumidor (capa de servicios externos en la frontera declarada, capa de decisión determinista del dominio sin IA) | `stack-guardian` | — |
| `integration` | Recorrer el **journey completo** de la release con **deps reales** del proyecto, no stubs | skill `verify` / `run` (+ MCP chrome-devtools) | `release-dod.md` |

Checklist de cierre: `references/release-dod.md`.

## Cómo proceder
1. Lee `build-state.json`; identifica la release y sus épicas (cruza con `docs/02-user-story-map/`).
2. Crea/actualiza la entrada en `releases[]` con `status: pending`.
3. Dispara los subagentes **en paralelo** sobre el diff acumulado (devuelven síntesis).
4. Corre el gate de **integración** con la skill `verify`/`run`: el journey completo, deps reales.
5. Cuando todos los gates pasan (o `null` cuando N/A) → `status: passed`, escribe los `gates` y
   `updated_by: releasing-a-version`. Si algo falla → `status: failed` y lista los hallazgos
   bloqueantes; el usuario los corrige como un slice normal (fix en `building-a-slice`) y se
   re-corre el Release Gate.

> **Opcional — conducir con workflow (releases grandes).** El fan-out del paso 3 puede conducirse con la
> plantilla `workflows/release-gate.workflow.js` (referencia, no obligatoria): SOLO paraleliza los 5 reviewers
> pesados; el gate `integration` (paso 4) sigue siendo **secuencial**, vía `verify`/`run` con **deps reales**,
> **fuera** del `parallel()`. El resultado se escribe igual en `releases[]` respetando **una escritura por
> entrada** y **validando contra el schema**; esta skill sigue siendo la única escritora. Parciales NO
> promueven a `passed`.

## Reglas duras
- **No dupliques el inner loop.** Aquí no se hace TDD ni se cierran gates por slice.
- **Integración con deps reales es obligatoria** para `status: passed` — es el gate que faltaba y
  por el que el producto "no funcionaba al terminar". No se acepta con todo stubbeado.
- Si una regla aquí contradice la metodología Trycore (`METODOLOGIA.md`), **gana la metodología**.
