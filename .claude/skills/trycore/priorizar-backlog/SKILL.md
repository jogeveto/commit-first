---
name: trycore-priorizar-backlog
description: Prioriza el backlog aplicando un framework (MoSCoW, RICE, Valor-Esfuerzo o Eisenhower). Genera `docs/05-priorizacion/<framework>-<fecha>.md` y actualiza el orden de filas en `backlog.md`. La skill APORTA conocimiento externo sobre los frameworks (la bibliografía base los menciona pero no los desarrolla). Path-scope principal: `docs/05-priorizacion/` + `docs/03-backlog/backlog.md`. Fase del pipeline: 6 de 7 (OPCIONAL — solo activar si el usuario pide priorización explícita; no se carga proactivamente en sesiones de edición de PRD/historias).
category: Discovery
tags: [priorizacion, moscow, rice, valor-esfuerzo, eisenhower, trycore]
---

# trycore-priorizar-backlog

Aplica un framework de priorización al backlog y registra la sesión en `docs/05-priorizacion/`. Después actualiza el orden de filas en `docs/03-backlog/backlog.md` si el usuario aprueba.

## Contexto (fuente: METODOLOGIA.md §7)

7 factores a considerar: valor del negocio, urgencia, dependencias, coste, riesgos, feedback del usuario, madurez tecnológica.

Frameworks soportados (la skill embebe las reglas porque la bibliografía base solo los nombra):

### MoSCoW
| Cat | Significado | % típico |
|---|---|---|
| Must | sin esto falla | ≤ 60% |
| Should | importante no crítico | ~20% |
| Could | deseable si hay capacidad | ~20% |
| Won't (esta vez) | explícito fuera | resto |

Regla: si todo es Must → reabrir discusión.

### RICE
`Score = (Reach × Impact × Confidence) / Effort`
- Impact: 0.25, 0.5, 1, 2, 3
- Confidence: 100, 80, 50, 20 (%)
- Effort: person-months

### Valor / Esfuerzo
Matriz 2×2. Orden: Quick wins → Big bets → Refactor → Skip.

### Eisenhower
Matriz urgencia × importancia. 4 cuadrantes: Hacer / Planificar / Delegar / Eliminar.

### Planning Poker (estimación, no priorización)
Fibonacci en cartas, votación privada, divergencias justifican, converger en 2 rondas. Solo si la complejidad no está clara aún.

## Inputs requeridos

- Framework a usar. Si no se especifica:
  1. Leer auto-memory tipo `project` por `prioritization_framework`.
  2. Si no hay, preguntar con `AskUserQuestion` (4 opciones).
- Participantes de la sesión (para el header del archivo).
- (Si RICE) las 4 estimaciones por historia. Si faltan, preguntar al usuario o sugerir hacerlo en una sesión separada.

## Plantilla

`templates/artefactos/priorizacion.template.md` (incluye los 4 bloques; usa solo el del framework elegido).

## Reglas duras

1. **Framework aplicado COMPLETO, no parcial.** Si MoSCoW: todas las historias caen en una de las 4 categorías, no quedan sin clasificar.
2. **Distribución sana**:
   - MoSCoW: Must ≤ 60% del esfuerzo total. Si supera, advertir.
   - RICE: el top 3 debe tener Score ≥ 2× el promedio del resto. Si no, el ordenamiento no está añadiendo señal.
3. **Disidencias registradas**: si en la sesión hubo desacuerdo, captarlo en la sección "Disidencias / preguntas abiertas".
4. **Próxima revisión**: cada sesión deja fecha tentativa para la siguiente priorización.
5. **El backlog.md se actualiza solo con aprobación explícita del usuario** (no automático).

## Flujo de la skill

1. **Detectar framework** (memory → pregunta).
2. **Cargar backlog actual** y todas las historias con sus frontmatters.
3. **Recorrer historias** y, para cada una, recopilar la información necesaria del framework:
   - MoSCoW: pregunta o infiere por valor/criticidad declarado.
   - RICE: pregunta los 4 valores.
   - Valor/Esfuerzo: pregunta valor (alto/bajo) y esfuerzo (alto/bajo).
   - Eisenhower: pregunta urgencia (sí/no) e importancia (sí/no).
4. **Calcular o agrupar** según el framework.
5. **Generar** `docs/05-priorizacion/<framework>-<YYYY-MM-DD>.md`.
6. **Mostrar resumen** al usuario y preguntar si quiere actualizar `backlog.md` con el nuevo orden.
7. Si aprueba: reordenar filas de `backlog.md` y actualizar la columna "Prioridad" de cada historia.

## Handoff

- **Agente revisor**: `priorizacion-auditor`. Verifica consistencia del framework (RICE bien sumado, MoSCoW sin sobre-Must, etc.).
- **Siguiente paso**: implementación. Las historias ordenadas se trabajan en sprint planning.

## Anti-pattern

- Decir "RICE" y solo asignar números sin formula → no es RICE.
- Tener un archivo de priorización pero no actualizar el orden de `backlog.md` → no sirvió.
- Re-priorizar cada semana sin avanzar implementación → señal de organización paralizada.
