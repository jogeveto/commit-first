---
name: trycore-mapear-flujos-navegacion
description: Genera flujos de navegación (user flows) en Mermaid por cada épica del proyecto, con trazabilidad 1:1 a las Historias de Usuario y sus AC. Úsalo cuando el usuario pida "flows", "user flows", "flujos de navegación", "diagramas de secuencia del producto", o tras completar el pipeline PRD→Backlog para entregar la dimensión UX/navegación. Path-scope principal: `docs/06-flows/`. Fase del pipeline: 7 de 7 (OPCIONAL / post-pipeline — solo activar si el usuario pide flows explícitamente; no se carga proactivamente en sesiones de edición de PRD/historias/backlog).
category: Discovery
tags: [flows, navegacion, mermaid, sequence-diagram, ux, trycore]
---

# trycore-mapear-flujos-navegacion

Genera un archivo de flujo por épica bajo `docs/06-flows/EP-XXX-<slug>.md`. Cada flujo describe en Mermaid la navegación del usuario para esa capability, mapeada arco-por-arco a las HU y AC ya existentes.

## Contexto (fuente: METODOLOGIA.md §5a)

El User Story Map cubre el **journey de actividades en orden cronológico**. Los AC en G/W/T cubren **estado-acción-resultado por escenario**. Pero ninguno describe el **grafo de navegación** entre pantallas/estados ni los ramales de error de manera holística. `flows` cierra esa frontera.

Granularidad: **un archivo por épica**. El backbone del Story Map ya descompone por épica; un flow por EP-XXX hereda esa unidad atómica.

Posición: **post-pipeline** (paso 6, opcional). Requiere PRD + épicas + HU + Story Map ya generados.

## Inputs requeridos (lectura, en este orden)

1. `docs/01-prd/<slug>.md` — objetivos, alcance, **Stakeholders** (única fuente válida de actores).
2. `docs/03-backlog/epicas.md` — lista de `EP-XXX` que delimitan los flujos a generar.
3. `docs/04-historias/HU-*.md` — frontmatter (`id`, `epica`) + AC en G/W/T (cada arco del flow se respaldará con un AC).
4. `docs/02-user-story-map/` — backbone cronológico para entender orden y agrupaciones por capability.

Si **cualquiera** de estos inputs falta o está vacío, **detener** y reportar el gap. No inventar pasos.

## Plantilla

`templates/artefactos/flow.template.md` — frontmatter + resumen + bloque Mermaid + tabla de trazabilidad.

## Reglas duras (rejection rules)

1. **Cada paso del flow mapea 1:1 a un AC existente**. En el diagrama, cada arco lleva `%% HU-XXX` como comentario inmediatamente antes del arco. Sin esa marca el arco se considera inventado.
2. **Cero pasos sin AC**. Si una transición que el flow necesita NO está cubierta por ningún AC, **detener**, reportar el gap y proponer al usuario crear el AC antes de continuar.
3. **Actores válidos: solo los del PRD §Stakeholders**. Cualquier nombre de actor que no aparezca en el PRD invalida el flow.
4. **Cobertura por flow**: happy path + ≥1 ramal de error + ≥1 edge case.
5. **Cobertura por épica**: toda HU declarada con `epica: EP-XXX` en su frontmatter debe aparecer referenciada al menos una vez en el flow correspondiente. HU que no aparece → reportar gap.
6. **Mermaid debe parsear**. Usar `sequenceDiagram` para interacciones actor↔sistema multi-paso; usar `flowchart TD` cuando domina la ramificación con decisiones. No mezclar ambos en un solo bloque.
7. **No sobrescribir flows previos sin confirmación**. Si `docs/06-flows/EP-XXX-*.md` ya existe, leer su frontmatter y preguntar (mantener / reescribir / diff manual).

## Flujo de la skill

1. **Detectar contexto**: leer `CLAUDE.md` del proyecto + memory `project`. Verificar la existencia de los 4 inputs.
2. **Listar épicas** desde `epicas.md`. Para cada EP-XXX, identificar las HU asociadas leyendo el frontmatter de `docs/04-historias/HU-*.md`.
3. **Para cada épica** (o solo la indicada por el usuario):
   1. Extraer actores desde el PRD §Stakeholders.
   2. Recolectar todas las HU + AC de esa épica.
   3. Decidir el tipo de diagrama:
      - `sequenceDiagram` si hay actor(es) interactuando con el sistema en pasos secuenciales.
      - `flowchart TD` si domina la lógica de decisión (varias ramas según condición).
   4. Construir el flow cubriendo happy + error + edge.
   5. Anotar cada arco con `%% HU-XXX`.
   6. Validar internamente: ¿alguna HU de la épica no aparece? → marcar gap y reportar.
   7. Escribir `docs/06-flows/EP-XXX-<slug>.md` usando la plantilla.
4. **Reportar al usuario** por cada flow: HU cubiertas, gaps detectados, próximo paso.

## Formato de salida (por flow)

```markdown
---
id: flow-{{NUM}}-{{slug}}
epica: EP-{{NUM_EPICA}}
historias_cubiertas: [HU-{{NUM_HU_1}}, HU-{{NUM_HU_2}}]
---

# Flow {{NUM}} — {{Nombre de la capability}}

## Resumen
<2-3 líneas: actor principal, objetivo, condición de éxito>

## Diagrama

```mermaid
sequenceDiagram
  %% HU-001
  Actor->>Sistema: ...
  ...
```

## Trazabilidad

| Paso | HU | AC |
|---|---|---|
| 1 | HU-001 | AC-1 |
```

## Handoff

- **Agente revisor**: `flows-auditor`. Si los hooks PostToolUse están activos (`TRYCORE_AUTO_AUDIT=true`), se dispara automáticamente al escribir cualquier archivo en `docs/06-flows/`. Si no, sugerir `/trycore:revisar`.
- **Trazabilidad cross-document**: `trazabilidad-auditor` sigue cubriendo AC→HU→Épica→Objetivo. `flows-auditor` cubre arco-de-flow→HU. Se complementan, no se duplican.

## Memoria

- **NO** escribas auto-memory desde esta skill. Si el equipo decide convenciones nuevas (ej. "siempre usar `flowchart LR` en lugar de `TD`"), sugerir al usuario añadirlo a `CLAUDE.md`.

## Ejemplo de invocación

Usuario: *"Genera los flujos de navegación del proyecto."*

1. Skill verifica que existen `prd.md`, `epicas.md`, `HU-*.md`, story-map. Falta uno → stop con reporte.
2. Encuentra 4 épicas (`EP-001..EP-004`) con HU asociadas.
3. Para cada épica:
   - `EP-001` (Búsqueda) con HU-001, HU-002, HU-003 → genera `docs/06-flows/EP-001-busqueda.md` con `sequenceDiagram`, happy/error/edge, `%% HU-001/002/003` en arcos, tabla de trazabilidad.
   - Repite para `EP-002..EP-004`.
4. Reporta: "Generados 4 flows. Gaps detectados: HU-008 (sin referencia en EP-003), AC inexistente para arco 'Recuperar contraseña expirada'. ¿Crear AC ahora o continuar?"

## Anti-pattern

- Generar un flow global único (`flows.md`) con todos los caminos del producto → crece sin gobierno y se vuelve ingobernable.
- Inventar pasos que "tienen sentido" pero no tienen AC → rompe la trazabilidad, es la única razón por la que esta skill existe.
- Usar wireframes ASCII o imágenes adjuntas → el repo es markdown puro, agnóstico, auditable. Mermaid es el contrato.
