---
name: "BUILD: Reflect"
description: Reflexión post-slice (ciclo autocorrectivo). Tras cerrar/archivar un slice, detecta convenciones aprendidas y errores recurrentes, PROPONE viñetas para el bloque de aprendizajes de CLAUDE.md (se aplican SOLO con tu aprobación) y estampa el slice como reflexionado. Lo dispara el hook reflect-nudge.sh, pero puedes invocarlo cuando quieras.
category: Workflow
tags: [reflexion, conocimiento, ciclo-autocorrectivo, build-harness, trycore]
---

Captura el **conocimiento tribal** de un slice recién cerrado antes de que se pierda. Un hook bash no
puede razonar qué se aprendió; **tú (el modelo) sí**. Tu trabajo: detectar convención nueva o error
recurrente, **proponérselo al usuario**, y —solo con su aprobación— escribirlo al bloque
`trycore-build-learnings` de `CLAUDE.md`. Nunca escribes sin confirmación.

Esta es la mejora del **pilar "ciclo autocorrectivo"**: el hook `reflect-nudge.sh` (evento `Stop`)
solo **sugiere** ejecutar esto cuando hay slices archivados sin reflexionar; el razonamiento vive
aquí, en el modelo.

---

## Preflight

```bash
test -f .claude/.build-harness-version || echo "NOT_INSTALLED"
command -v python3 >/dev/null 2>&1 || echo "NO_PYTHON3"
```

**Si `NOT_INSTALLED`:**

> Este proyecto no tiene el arnés de construcción instalado. Ejecuta primero `trycore-build init`.

Stop aquí si no está instalado o si falta `python3`.

---

## Fase 1: Identificar slices sin reflexionar

Lee `.claude/state/build-state.json` y filtra las entradas de `history[]` con `reflected != true`:

```bash
python3 - <<'PY'
import json, os, sys
p = ".claude/state/build-state.json"
if not os.path.exists(p):
    print("NO_STATE"); sys.exit(0)             # estado ausente → nada que reflexionar, salir
try:
    d = json.load(open(p))
except Exception as e:
    print("CORRUPT_STATE", e); sys.exit(0)      # JSON inválido → reportar y STOP, NO escribir
pend = [h for h in (d.get("history") or []) if isinstance(h, dict) and h.get("reflected") is not True]
for h in pend:
    print(h.get("epica"), "·", h.get("openspec_change"), "·", h.get("branch"), "·", ",".join(h.get("hus") or []))
print("TOTAL", len(pend))
PY
```

**Si `NO_STATE`:** no hay estado → nada que reflexionar; termina. **Si `CORRUPT_STATE`:** el estado está
corrupto → repórtalo y **detente sin escribir** (no estampes ni edites). **Si `TOTAL 0`:** informa "No hay
slices pendientes de reflexión ✅" y termina. No inventes trabajo.

Si hay varios, procésalos **de uno en uno** (el más reciente primero), o pregunta al usuario cuál.

---

## Fase 2: Recolectar señal del slice

Para el slice elegido, reúne evidencia **real** (no especules):

1. **El change OpenSpec**: `openspec show <openspec_change>` y/o los archivos del change
   (proposal/tasks). ¿Qué se decidió y qué tasks costaron más?
2. **El diff de la rama**: `git log --oneline <branch>` y `git diff main...<branch> --stat` (si la
   rama o sus commits existen). ¿Qué patrón de código se repitió? ¿Qué se reescribió varias veces?
3. **La sesión actual**: qué hooks se dispararon más de una vez (p.ej. `stack-guard.sh`,
   `gitflow-guard.sh`, `lint-typecheck.sh`), qué gates retrocedieron, qué correcciones se repitieron.

Con esa evidencia, detecta **0–3** aprendizajes candidatos, de estas categorías:

- **Convención nueva**: un patrón de código/estructura/nombrado que se repitió y conviene fijar para
  el equipo (ej. "los handlers validan en el borde con el esquema X antes de tocar el dominio").
- **Error recurrente**: algo que un gate o hook atrapó >1 vez y vale la pena prevenir
  (ej. "recordar correr migraciones antes del journey-smoke").
- **Fricción del propio arnés**: si el aprendizaje es sobre el arnés (un gate confuso, un paso que
  sobra), NO lo escribas a CLAUDE.md — anótalo como candidato para `internal/skills/auditar-arnes`
  y dilo al usuario.

Si no hay nada que valga la pena fijar, es válido **no proponer aprendizajes** (solo estampar, Fase 5).

---

## Fase 3: Proponer (con aprobación explícita)

Por **cada** aprendizaje candidato, usa **AskUserQuestion** para que el usuario lo apruebe, edite o
descarte. Presenta cada uno como una **viñeta concreta y accionable**, lista para CLAUDE.md. Ejemplo
de opciones: "Agregar tal cual" (Recomendado) · "Editar" · "Descartar".

**Reglas:**
- Una viñeta = una convención. Concreta, verificable, en presente imperativo. Nada vago.
- No propongas conocimiento que ya está en el bloque (léelo antes). No dupliques.
- Si un aprendizaje es **personal/cross-proyecto** (no de equipo), ofrécelo para **auto-memory**
  (`type: project`/`user`) en vez de CLAUDE.md — pregunta cuál destino.

---

## Fase 4: Aplicar SOLO lo aprobado al bloque de aprendizajes

Para las viñetas aprobadas, **inserta** (no reemplaces) dentro del bloque marcado de `CLAUDE.md`,
**justo antes** de `<!-- END trycore-build-learnings -->`:

```markdown
- (EP-XXX) <viñeta aprobada>
```

- Prefija cada viñeta con la épica del slice `(EP-XXX)` para trazabilidad.
- **NO toques nada fuera del bloque** `trycore-build-learnings`. En particular, NO modifiques el
  bloque `trycore-build-harness` ni sus `{{placeholders}}` de dominio (eso es de `/build:onboard`).
- Si el bloque `trycore-build-learnings` no existe (instalación vieja), pide correr
  `trycore-build update` antes de continuar.
- Las viñetas son cambio de equipo: se revisan en PR como cualquier otra línea de `CLAUDE.md`.

---

## Fase 5: Estampar el slice como reflexionado

Marca el/los slice(s) procesado(s) en `history[]` para que el nudge calle. Usa la hora UTC real:

```bash
NOW="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
python3 - "$NOW" "<openspec_change>" <<'PY'
import json, sys, os, tempfile
now, change = sys.argv[1], sys.argv[2]
p = ".claude/state/build-state.json"
try:
    d = json.load(open(p))
except Exception as e:
    print("ABORT: estado ilegible, no estampo:", e); sys.exit(1)
for h in d.get("history") or []:
    if isinstance(h, dict) and h.get("openspec_change") == change:
        h["reflected"] = True
        h["reflected_at"] = now
# Validación contra el schema ANTES de persistir (si jsonschema está disponible); aborta si no valida.
try:
    import jsonschema
    schema = json.load(open(".claude/state/build-state.schema.json"))
    jsonschema.Draft202012Validator(schema).validate(d)
except ImportError:
    pass  # sin jsonschema: se omite la validación profunda (no se relaja la escritura atómica)
except Exception as e:
    print("ABORT: el estado modificado NO valida contra el schema, no escribo:", e); sys.exit(1)
# Escritura ATÓMICA (una transición = una escritura): tmp + os.replace.
fd, tmp = tempfile.mkstemp(dir=os.path.dirname(p) or ".", prefix=".build-state.", suffix=".tmp")
with os.fdopen(fd, "w") as out:
    json.dump(d, out, indent=2, ensure_ascii=False)
os.replace(tmp, p)
print("estampado:", change)
PY
```

Estampa **aunque no haya habido aprendizajes** (reflexionar y no encontrar nada también cierra el
ciclo). Repite por cada slice procesado.

---

## Fase 6: Confirmación

```
## ✓ Reflexión completada

Slice:        <EP-XXX> (<openspec_change>)
Aprendizajes: <N agregados a CLAUDE.md> · <M a auto-memory> · <0 si solo se estampó>
Estampado:    reflected=true

Pendientes de reflexión restantes: <K>
```

Si quedan slices pendientes (`K > 0`), ofrece continuar con el siguiente.

---

## Guardrails

- **Aprobación explícita siempre.** Nunca escribes a CLAUDE.md sin que el usuario apruebe cada
  viñeta. Espeja la regla del scaffold: el arnés propone, el humano confirma.
- **Evidencia, no invención.** Cada aprendizaje sale del change/diff/sesión reales. Si no hay
  evidencia, no propongas.
- **No reescribas.** Inserta viñetas; jamás borres ni reemplaces aprendizajes previos del equipo.
- **No toques el paquete.** No modifiques agentes/skills/hooks del arnés (eso es mantenimiento, ver
  `internal/skills/auditar-arnes`).
- **Estampa siempre** al terminar, para no re-molestar con el mismo slice.
- Si algo aquí contradice `METODOLOGIA.md`, **gana la metodología**.
