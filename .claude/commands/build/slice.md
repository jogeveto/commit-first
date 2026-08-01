---
name: "BUILD: Slice"
description: Punto de entrada del inner loop. Abre o continúa un slice (épica EP-XXX) y conduce el pipeline DoR → change → TDD → smoke → api/data → dod → PR+archive delegando en la skill building-a-slice (o el agente build-orchestrator para épicas multicapa). Respeta el orden estricto de gates y el scaffold como precondición.
category: Workflow
tags: [build-harness, inner-loop, slice, trycore]
---

Lanza el **inner loop** de construcción sobre una épica. Este comando es un **adaptador delgado**: no
reimplementa el pipeline — **delega** en la skill `building-a-slice` (motor del inner loop) y en `opsx:*`
(motor de changes). Si algo aquí contradice `METODOLOGIA.md`, **gana la metodología**.

**Entrada:** `EP-XXX` o una descripción de la épica. Si viene vacío, usa **AskUserQuestion** para elegir la
épica desde `docs/03-backlog/epicas.md`.

---

## 1. Preflight

```bash
test -f .claude/.build-harness-version || echo "NOT_INSTALLED"
command -v python3 >/dev/null 2>&1 || echo "NO_PYTHON3"
```

**Si `NOT_INSTALLED`:** este proyecto no tiene el arnés instalado → ejecuta `trycore-build init` y vuelve.
Stop si no está instalado o falta `python3`.

---

## 2. Leer el estado y decidir punto de entrada

```bash
python3 - <<'PY'
import json, os, sys
p = ".claude/state/build-state.json"
if not os.path.exists(p): print("NO_STATE"); sys.exit(0)
try: d = json.load(open(p))
except Exception as e: print("CORRUPT_STATE", e); sys.exit(0)
s = d.get("active_slice")
if not s:
    print("START dor")
else:
    g = s.get("gates", {})
    abierto = next((k for k, v in g.items() if v is False), None)
    print(f"RESUME {s.get('epica')} fase={s.get('phase')} primer_gate_abierto={abierto}")
PY
```

- `NO_STATE`/`CORRUPT_STATE` → reporta y detente (no escribas).
- `START dor` → no hay slice activo: arranca en **dor** con la épica objetivo.
- `RESUME …` → ya hay un slice activo: **reanuda en su primer gate abierto** (no abras otro: el modelo es
  secuencial, un solo slice activo).

---

## 3. Precondición de scaffold (y fuente de diseño si hay UI)

Antes de escribir código de slice, verifica los gates de proyecto:

- `scaffold.confirmed` debe ser `true`. Si es `false` → **STOP**: delega en la **Fase 0** de `building-a-slice`
  (pregunta explícita; el arnés **no genera** el scaffold). No abras el slice.
- Si el proyecto tiene UI, `design_source.confirmed` debe ser `true` (Fase 0-bis). Si no → **STOP** igual.

El hook `scaffold-guard.sh` respalda esto en tiempo real.

---

## 4. Conducir el pipeline (delegar)

Invoca la skill **`building-a-slice`** para conducir el inner loop. Para una épica **multicapa / grande**
(superó el gate de tamaño → `sub_slices[]`), invoca el agente **`build-orchestrator`** (trabaja por fases
encadenadas y, opcionalmente, conduce la exploración solo-lectura con `workflows/explore-fanout.workflow.js`).

- **No** ejecutes `opsx:apply` directamente ni saltes gates: el orden es estricto
  (`dor → change → tdd → smoke → api/data → dod → pr`).
- **No** dispares reviewers pesados aquí (`security`, `smell`, `ux`, `coherence`, `stack_arch`): pertenecen
  al **Release Gate** (`/build:release`). Hacerlo por slice rompería el modelo de dos loops.

---

## 5. Resumen

Al terminar el paso, resume: fase actual, gates cerrados/abiertos y el siguiente gate. Si la épica quedó
archivada, recuerda el default del Release Gate (ver `/build:release`).

---

## Guardrails

- **Un solo slice activo** (secuencial). No abras un segundo mientras haya `active_slice`.
- **Orden estricto de gates**; un gate no se salta. `dod` exige `wiring_verified: true`.
- **Sin scaffold confirmado, no hay slice** (el arnés lo exige pero no lo genera).
- **Agnóstico**: este comando no asume dominio; lo específico entra por `/build:onboard` y `stack-allowlist.json`.
- Si algo contradice `METODOLOGIA.md`, **gana la metodología**.
