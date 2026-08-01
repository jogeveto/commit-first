---
name: "BUILD: Release"
description: Punto de entrada del outer loop. Corre el Release Gate UNA vez sobre el diff acumulado de una release — los 5 reviewers pesados (security, smell, ux, coherence, stack_arch) en paralelo + integración secuencial con deps reales — delegando en la skill releasing-a-version. No duplica el inner loop (ni TDD ni gates por slice).
category: Workflow
tags: [build-harness, outer-loop, release-gate, trycore]
---

Lanza el **outer loop**: las revisiones profundas que corren **una sola vez por release** sobre el diff
acumulado, no por épica. Adaptador delgado: **delega** en la skill `releasing-a-version` (no la reimplementa).
Si algo aquí contradice `METODOLOGIA.md`, **gana la metodología**.

**Entrada:** `release_id` (p.ej. `R1-mvp`) o, si viene vacío, infiérelo del default computado tras archivar
(ver §4 de la metodología y el nudge de `release-gate-nudge.sh`).

---

## 1. Preflight

```bash
test -f .claude/.build-harness-version || echo "NOT_INSTALLED"
command -v python3 >/dev/null 2>&1 || echo "NO_PYTHON3"
```

Stop si no está instalado o falta `python3`.

---

## 2. Identificar la release y sus épicas

Lee `build-state.json` y cruza con `docs/02-user-story-map/` para resolver qué épicas componen la release.
Crea/actualiza la entrada en `releases[]` con `status: pending` (lo escribe `releasing-a-version`, única
escritora de `releases[]`).

---

## 3. Computar el diff acumulado

El alcance es el rango de commits de **todas** las épicas de la release: desde el merge anterior a la primera
épica de la release hasta `main`.

---

## 4. Fan-out de reviewers pesados (en paralelo)

Dispara **en paralelo** sobre ese diff los 5 reviewers (cada uno devuelve síntesis):

| Gate | Subagente |
|---|---|
| `security` | `security-reviewer` |
| `smell` | `simple-design-reviewer` |
| `ux` (o `null` si sin UI) | `ux-krug-reviewer` |
| `coherence` | `coherence-three-way` |
| `stack_arch` | `stack-guardian` |

Opcionalmente conduce este fan-out con la plantilla `skills/releasing-a-version/workflows/release-gate.workflow.js`
(referencia). La plantilla **solo** paraleliza los 5 reviewers.

---

## 5. Integración SECUENCIAL (fuera del paralelo)

Corre el gate `integration` con la skill `verify`/`run` (+ MCP chrome-devtools): el **journey completo** de la
release end-to-end con **dependencias reales**, no stubs. **No** va dentro del `parallel()` ni delega en un
reviewer. Es el gate **no negociable**: sin él, no hay release.

---

## 6. Síntesis y escritura

- Todos los gates ✓ (o `null` cuando N/A) **y** `integration` ✓ → `releases[].status: passed`; escribe
  `gates` y `updated_by: releasing-a-version` (valida contra `build-state.schema.json` antes de persistir;
  una escritura por entrada). **Parciales no promueven a `passed`.**
- Algo ✗ → `status: failed` con los hallazgos bloqueantes; el humano los corrige **como un slice normal**
  (`/build:slice` / `building-a-slice`) y se **re-corre** el Release Gate.

---

## Guardrails

- **Es outer loop.** Correr reviewers pesados **por slice** rompería el modelo de dos loops — aquí corren una
  vez por release (`O(releases)`).
- **No dupliques el inner loop**: no se hace TDD ni se cierran gates por slice.
- **`integration` con deps reales es obligatorio**; no se acepta con todo stubbeado.
- Delega en `releasing-a-version`; **no** la reimplementa. Si algo contradice `METODOLOGIA.md`, gana la metodología.
