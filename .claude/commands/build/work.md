---
name: "BUILD: Work"
description: Router de entrada (classify-and-act) del arnés. Clasifica el trabajo entrante y enruta a la skill correcta — building-a-micro-change (mantenimiento), building-a-slice (épica/producto nuevo) o releasing-a-version (Release Gate) — codificando los límites duros del micro-change y el default del Release Gate. Es RUTEO, no política: no ejecuta el pipeline, no toca el estado ni crea ramas.
category: Workflow
tags: [build-harness, router, classify-and-act, trycore]
---

**Router puro.** Decide *qué carril* aplica y **delega** en la skill correspondiente. NO ejecuta el pipeline,
NO escribe `build-state.json`, NO crea ramas. Codifica como **ruteo** (no política nueva) el *decision gate*
del micro-change y el default del Release Gate de `METODOLOGIA.md` (§2, §4). Si algo contradice la metodología,
**gana la metodología**.

**Entrada:** una descripción del trabajo a hacer.

---

## 1. Preflight

```bash
test -f .claude/.build-harness-version || echo "NOT_INSTALLED"
```

Si `NOT_INSTALLED` → ejecuta `trycore-build init` y vuelve.

---

## 2. Clasificar (decision gate)

Aplica las reglas en orden:

1. **¿Mantenimiento sin capacidad nueva?** — typo, ajuste de copy/config/docs, bump de dependencia **ya
   permitida**, o fix de **pocas líneas** sin nueva capacidad **Y** sin ninguno de los límites duros del
   paso 3 → carril **`building-a-micro-change`** (`fix/*`|`chore/*` → PR, **sin** abrir `active_slice`).
2. **¿Producto nuevo / una épica?** — capacidad nueva, o cualquier límite duro cruzado → carril
   **`building-a-slice`** (una épica `EP-XXX` = un slice = un change = una rama = un PR). Si no hay épica aún,
   el trabajo vuelve a discovery para crearla.
3. **¿Toca correr el Release Gate?** — la épica recién archivada **cierra una línea de release** del Story Map,
   o hay **≥ 2 épicas archivadas** desde el último entry de `releases[]` → sugiere el carril
   **`releasing-a-version`** (outer loop). (El destino es la skill existente; este router no depende de
   `/build:release`.)

---

## 3. Límites duros del micro-change (escalan a épica)

Si el cambio **añade una dependencia nueva**, **crea un endpoint/API nuevo**, o **toca lógica de dominio o el
modelo/invariantes de datos** → **deja de ser micro-change** y se enruta a **`building-a-slice`** (épica).
**Ante la duda, SIEMPRE épica.**

---

## 4. Actuar (delegar) — con degradación headless

- **Con TTY**: confirma la clasificación con **una sola** `AskUserQuestion` (ofrece el carril propuesto como
  primera opción "(Recomendado)") y luego invoca la skill elegida.
- **Sin TTY / headless / entrada ausente**: **no bloquees**. Clasifica determinísticamente y emite por stdout
  `{clasificación, skill recomendada, criterio que disparó la rama}`. Si es **ambiguo**, aplica el **default
  duro**: escalar a épica → `building-a-slice`, declarándolo explícitamente.

---

## Guardrails

- **Solo ruteo.** No corres el pipeline, no tocas el estado, no creas ramas: eso es de las skills destino.
- **Ante la duda, épica.** Nunca degrades un cambio con límite duro a micro-change.
- **No dupliques gates** ni saltes el orden: las skills destino los gobiernan.
- **Agnóstico**: vocabulario genérico del arnés; lo específico del dominio entra por `/build:onboard`.
- Si algo contradice `METODOLOGIA.md`, **gana la metodología**.
