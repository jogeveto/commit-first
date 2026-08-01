# `workflows/` — plantillas de workflow del inner loop (`building-a-slice`)

> **Plantillas, NO scripts a correr verbatim.** Los archivos `*.workflow.js` de esta carpeta son
> **referencia** conducida por la skill `building-a-slice`. Si contradicen `METODOLOGIA.md`, **gana la
> metodología**.

## Reglas duras (todas las plantillas las cumplen)
1. **Solo para épicas grandes.** Los workflows del inner loop son **OPT-IN** y solo para épicas troceadas por
   el gate de tamaño (`sub_slices[]` no vacío). **Nunca** en el camino caliente ≤ ~20 min de una épica
   atómica: inflaría el inner loop barato.
2. **Read-only sobre el estado.** Ninguna plantilla escribe `build-state.json`. El único escritor de los
   gates del slice sigue siendo `build-orchestrator` (y los agentes dueños de cada gate). Las plantillas
   **devuelven un diagnóstico**; la sesión/orquestador aplica el mapeo respetando el protocolo: **una
   transición = una escritura**, gates **monótonos** (`false`→`true` solo por su agente; retroceso solo ante
   fallo) y **validar contra `build-state.schema.json` tras escribir**.
3. **Subagentes de exploración = solo-lectura.** No editan código ni estado; su única salida es síntesis
   condensada. El **cableado lo hace la sesión**, no subagentes en paralelo.
4. **Agnóstico.** Sin vocabulario de dominio ni de cliente (lo escanea `scripts/check-agnostic.sh`, que
   incluye `*.js`). Adapta áreas/rutas a tu stack, no incrustes nombres de dominio.

## Plantillas
- **`explore-fanout.workflow.js`** — exploración "ancho antes que profundo" por área (fan-out → síntesis),
  gated por `sub_slices[]`. Contrato detallado en `../references/exploration-fanout.md`.
- **`wiring-verify.workflow.js`** — conducción adversarial del gate `wiring_verified` (envuelve, read-only, al
  agente `wiring-adversarial-verifier`); devuelve el veredicto, no escribe el gate.
