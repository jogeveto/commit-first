# Definition of Ready (DoR) — checklist de entrada

Una **épica** (`EP-XXX`) **no entra a construcción** hasta cumplir TODO esto. La unidad del slice es
la épica; lo de abajo aplica a la épica y a **cada HU** que cubre (`hus[]`). Lo valida `dor-dod-gatekeeper`.

- [ ] **Épica válida**: `EP-XXX` existe en `docs/03-backlog/epicas.md` con trazabilidad a objetivos del PRD.
- [ ] **HU enumeradas**: la épica tiene ≥1 HU; todas las que entran se listan en `hus[]`.
- [ ] **Frontmatter completo** en cada `docs/04-historias/HU-XXX.md`: `id, titulo, epica, prioridad, complejidad, estado` y `estado: lista`.
- [ ] **AC en Given/When/Then** por HU, **proporcional a `complejidad`** (cubre los modos de fallo que *realmente existen*, no una cuota fija): `trivial`/baja → **1–2** (happy + el error/edge crítico si existe); `media` → **3** (happy + error + edge); `alta` → **3–5** (cobertura completa). Regla dura Trycore: si existe una rama de error/edge, **debe** tener su escenario (lo que se elimina es fabricar 3–5 para una HU trivial).
- [ ] **INVEST** por HU: pasa los 6 criterios (Independent, Negotiable, Valuable, Estimable, Small, Testable).
- [ ] **Dependencias resueltas**: las épicas/HU de las que depende están archivadas (`history[]`) o explícitamente no bloquean. **Excepción dura — cimiento:** si la dependencia es **infraestructura fundacional** (autenticación, acceso a datos, arquitectura base, design-system/componentes base), la cláusula "explícitamente no bloquean" **NO aplica**: debe estar **construida y archivada** antes (ver criterio "Cimiento construido").
- [ ] **Cimiento construido (épicas de negocio)**: si esta épica es `layer: business`, todo el cimiento que arrastra (auth, acceso a datos, arquitectura base, design-system/componentes base) ya existe como épica(s) `layer: foundational` **archivada(s)** en `history[]`. Si arrastra cimiento no construido → **STOP**: extráelo a una épica fundacional previa y constrúyela primero. Las épicas fundacionales se priorizan **antes** que las de negocio.
- [ ] **Tamaño acotado (gate de descomposición)**: si la épica supera el umbral —heurística por defecto **> 3 HU** ó **≥ 3 capas tocadas** (configurable por proyecto)— **no entra como slice único**: se descompone en `sub_slices[]` verificables construidos de a uno, con `journey_smoke` verde entre cada uno. El umbral es proporcional (no cuota rígida): una épica de 1 capa y pocas HU entra directa.
- [ ] **Cabe en el stack** del PRD §7 (no requiere tecnología fuera de `stack-allowlist.json`).
- [ ] **Datos de prueba disponibles** o identificables (p.ej. los datos de ejemplo / fixtures sintéticos del dominio del consumidor).
- [ ] **Fuente de diseño identificada (slices con UI)**: la fuente visual de verdad del slice
  (el `DESIGN_SOURCE` del dominio) está declarada y confirmada (`design_source.confirmed`), y este
  slice apunta a la(s) pantalla(s) equivalente(s). No se construye UI fuera de la fuente declarada.
- [ ] **Clasificación `layer`**: `foundational` (auth/datos/design-system/arquitectura base) | `business`. Se escribe en `active_slice.layer`. Gatea el front paralelo (foundational nunca en paralelo).
- [ ] **`files_scope`**: globs de los archivos que la épica tocará (p.ej. `src/reports/**`). Fuente de la disjunción inter-épica. Se escribe en `active_slice.files_scope`.

**Si todo ✓** → `dor-dod-gatekeeper` abre `active_slice` en `build-state.json` con `epica`, `hus[]`,
`phase: dor`, `gates.dor: true` y el resto en `false` —incluido **`wiring_verified: false`**—
(`fidelity`/`api` en `null` si la épica no toca UI/endpoints; `fidelity` arranca en `false` si toca UI).
**Si algo ✗** → no se abre el slice; se reporta qué falta y se vuelve a discovery (Trycore).
