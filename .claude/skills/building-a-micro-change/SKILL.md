---
name: building-a-micro-change
description: Use for genuine maintenance that is NOT new product capability — a typo, a copy/string tweak, a dependency version bump within the stack allowlist, an infra/config/docs change, or a small bug fix of a few lines that adds no new capability. Lightweight lane — branch fix/*|chore/* → change → regression test only if behavior changes → PR — WITHOUT opening active_slice, an epic (EP-XXX), or an OpenSpec change. HARD LIMITS: escalate to building-a-slice (a full epic) if the change adds a new dependency, creates a new public API/endpoint, or changes domain logic or the data model/invariants. The epic stays the unit for product construction; this lane is out-of-band maintenance only.
---

# Micro-change (mantenimiento) — carril ligero

Carril para **mantenimiento que no es construcción de producto nueva**. Espeja la filosofía del
arnés: la épica es la unidad de **construcción**, pero un typo o un bump de dependencia **no son
construcción** — forzarlos por las 8 fases de `building-a-slice` es ceremonia desproporcionada. Este
carril les da una vía corta **sin** diluir los guardarraíles deterministas.

> **Si una regla aquí contradice la metodología Trycore (`METODOLOGIA.md`), gana la metodología.**

## Paso 0 · Decision gate (obligatorio) — ¿es esto un micro-change?

Un cambio califica como micro-change **solo si cumple TODO**:

- **No añade capacidad de producto nueva.** Corrige, ajusta o mantiene algo que ya existe.
- **Alcance acotado:** pocas líneas / una sola preocupación. No toca múltiples módulos a la vez.
- **Proyecto en fase `active`** (el scaffold runnable ya existe y está confirmado). El carril **no**
  es para arrancar proyectos (eso es la Fase 0 de `building-a-slice`).

Ejemplos típicos (neutros): corregir un typo o un texto visible; ajustar un valor de configuración;
actualizar la versión de una dependencia **ya presente en la allowlist**; cambios de docs; un fix de
una a pocas líneas que repara un comportamiento sin introducir nada nuevo.

### Límites DUROS — si el cambio cruza **cualquiera**, STOP: esto es una épica

Escala a `building-a-slice` (abre una épica `EP-XXX` con su DoR) si el cambio:

1. **Añade una dependencia nueva** (fuera de `stack-allowlist.json`). *Lo bloquea además
   `stack-guard.sh` de forma determinista.*
2. **Crea un endpoint o una API pública nueva.**
3. **Cambia lógica de dominio** o el **modelo/invariantes de datos**.
4. **Desborda el alcance acotado** (introduce capacidad, toca muchos archivos, mezcla preocupaciones).

Ante la duda, **es una épica**. El carril micro-change nunca es un atajo para esquivar gates de
producto.

## Pipeline ligero

1. **Rama tipada.** Crea `fix/<slug>` (corrección) o `chore/<slug>` (infra/config/docs/bump).
   `gitflow-guard.sh` ya exige rama tipada y prohíbe commit/push directo a `main`.
2. **Aplica el cambio acotado.** Mantente dentro de los límites duros. Si al implementar descubres
   que cruzas uno, **detente y escala** a `building-a-slice`.
3. **Test de regresión — solo si cambia comportamiento.** Si el micro-change repara un bug,
   añade/ajusta un test que falle antes y pase después (red→green del fix, delega en
   `superpowers:test-driven-development`). Para cambios **no conductuales** (typo en copy, docs,
   config) **no** se exige test.
4. **PR a `main`.** Abre el Pull Request (`gitflow-guard.sh` impide la integración por push directo).
   En la descripción del PR indica que es un micro-change y por qué califica (qué límite NO cruza).

## Qué se mantiene y qué se salta

| Se mantiene (gratis, vía hooks deterministas) | Se salta (por diseño) |
|---|---|
| `gitflow-guard` (rama tipada + PR) | DoR formal (escenarios G/W/T, INVEST) |
| `stack-guard` (no dependencia nueva) | OpenSpec change + bloque `## Trazabilidad` |
| `lint-typecheck` (estilo + typecheck incremental) | `journey_smoke`, `api`, `data`, DoD reducido |
| | Apertura de `active_slice` / decisión de Release Gate |

`scaffold-guard` no aplica: no hay slice activo y el carril exige proyecto en fase `active` (scaffold
ya confirmado).

## Estado y trazabilidad

El micro-change **no escribe** `build-state.json` — es mantenimiento fuera de banda, trazado por el
historial de git y el PR. No entra a `history[]`, así que `reflect-nudge.sh` **no** sugiere
reflexionar por él (no hay aprendizaje de épica que capturar en un typo).

## Reglas duras

- **El decision gate es obligatorio.** Si dudas si algo es micro-change o épica, **es épica**.
- **Límites duros = STOP, no excepción.** Cruzar un límite obliga a escalar a `building-a-slice`;
  jamás se "fuerza" un micro-change para evitar el DoR.
- **Integración solo por PR** a `main` (lo respalda `gitflow-guard.sh`).
- Si una regla aquí contradice `METODOLOGIA.md`, **gana la metodología**.
