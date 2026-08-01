# Release Gate — checklist de salida (outer loop)

Una **release** (línea de release del Story Map) no se da por cerrada hasta cumplir TODO esto. Se
corre **una vez** sobre el diff acumulado de todas sus épicas. Resultado en `build-state.json` →
`releases[]`.

- [ ] **`security`** — `security-reviewer` sin hallazgos CRÍTICO/ALTO sobre el diff completo de la release; claves de servicios externos solo server-side; datos sensibles / PII regulados (según el PRD del consumidor) no persistidos crudos; salida de cualquier servicio externo/IA tratada como input no confiable y validada contra esquema antes de alimentar la capa de decisión.
- [ ] **`smell`** — `simple-design-reviewer` sin bloqueantes sobre el diff acumulado; 4 reglas de Beck respetadas.
- [ ] **`ux`** — `ux-krug-reviewer` ok sobre la UI ensamblada (o `null` si la release no tiene UI). Lighthouse/accesibilidad si la app corre.
- [ ] **`coherence`** — `coherence-three-way` confirma trazabilidad AC↔change↔código de **todas** las HU de **todas** las épicas de la release, sin huérfanos.
- [ ] **`stack_arch`** — `stack-guardian` confirma la arquitectura del PRD del consumidor: la capa de servicios externos/IA en la frontera declarada server-side (no decide), la capa de decisión determinista del dominio sin IA, sin claves de servicios externos en cliente.
- [ ] **`integration`** — el **journey completo** de la release se recorre end-to-end con **dependencias reales** del proyecto (servicios externos/IA y capa de decisión reales, según el PRD del consumidor), no stubs. Verificado con la skill `verify`/`run` (+ MCP `chrome-devtools`). Se corre **fuera** del fan-out
  paralelo de reviewers (es **secuencial**, con deps reales); ver `../workflows/release-gate.workflow.js`.

**Todo ✓ (o `null` cuando N/A)** → `releases[].status: "passed"`, escribe `gates` y `updated_by:
releasing-a-version`. **Algo ✗** → `status: "failed"`, lista hallazgos bloqueantes; se corrigen como
un slice normal en `building-a-slice` y se re-corre el Release Gate.

> El gate `integration` es el que faltaba en la era anterior: todos los gates por-slice estaban en
> verde y aun así el producto no caminaba de punta a punta. Sin `integration` con deps reales no hay
> release.
