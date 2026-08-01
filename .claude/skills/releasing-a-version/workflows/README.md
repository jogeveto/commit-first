# `workflows/` — plantillas de workflow del outer loop (`releasing-a-version`)

> **Plantillas, NO scripts a correr verbatim.** Conducidas por la skill `releasing-a-version`. Si contradicen
> `METODOLOGIA.md`, **gana la metodología**.

## Reglas duras
1. **Hogar primario de los workflows.** El Release Gate corre **una vez por release** (`O(releases)`), fuera
   del camino caliente del inner loop. **No duplica** el inner loop (ni TDD ni gates por slice).
2. **Read-only sobre el estado.** La plantilla devuelve veredictos; la skill `releasing-a-version` es la
   **única** que escribe `releases[]` (una entrada por release, validando contra `build-state.schema.json`,
   `updated_by: releasing-a-version`). Parciales **no** promueven a `passed`.
3. **`integration` fuera del paralelo.** Los 5 reviewers pesados van en `parallel()`; el gate `integration`
   (journey completo con **deps reales**) es **secuencial**, lo corre la skill `verify`/`run`, y **no** delega
   en un reviewer. Es el gate no negociable.
4. **Agnóstico.** Sin vocabulario de dominio/cliente (`scripts/check-agnostic.sh` incluye `*.js`).

## Plantillas
- **`release-gate.workflow.js`** — `parallel(5 reviewers)` → `integration` secuencial → síntesis a
  `releases[].gates.{security, smell, ux, coherence, stack_arch, integration}`.
