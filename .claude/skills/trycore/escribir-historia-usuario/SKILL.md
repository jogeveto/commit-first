---
name: trycore-escribir-historia-usuario
description: Escribe una Historia de Usuario en formato "Como [rol], quiero [acción], para [beneficio]" con frontmatter YAML obligatorio. La historia se guarda en `docs/04-historias/HU-XXX.md`. Úsalo para cada historia anticipada en el Story Map o en las épicas. Path-scope principal: `docs/04-historias/`. Fase del pipeline: 4a de 7 (obligatoria, suele ir en loop con escribir-criterios-aceptacion-bdd).
category: Discovery
tags: [user-story, historia-usuario, invest, trycore]
---

# trycore-escribir-historia-usuario

Produce `docs/04-historias/HU-XXX-<slug>.md` con la historia individual. Esta skill NO escribe los AC — los AC son el siguiente paso (`trycore-escribir-criterios-aceptacion-bdd`).

## Contexto (fuente: METODOLOGIA.md §3)

Formato canónico (no negociable):

```
Como [tipo de usuario específico],
quiero [realizar una acción concreta],
para [obtener un beneficio externo y visible].
```

## Inputs requeridos

- ID de la historia (auto-incrementar mirando `docs/04-historias/` actual).
- Épica madre (`EP-XXX`).
- Rol del usuario (específico, no "usuario").
- Acción.
- Beneficio.
- (Opcional) Prioridad y complejidad iniciales (se refinan después).

Si falta cualquiera, preguntar con `AskUserQuestion`.

## Plantilla

`templates/artefactos/historia-usuario.template.md`.

## Reglas duras

1. **Rol específico**: nada de "usuario", "cliente genérico". Si el equipo tiene buyer personas, anclarlo.
2. **Acción concreta** del usuario, no del sistema. "Quiero filtrar por categoría" ✓. "Quiero que el sistema filtre" ✗.
3. **Beneficio externo y visible** — si no se puede explicar, la historia no existe. Beneficios técnicos internos ("para mejor performance") no califican.
4. **Frontmatter YAML completo**: `id`, `titulo`, `epica`, `prioridad`, `complejidad`, `estado`. Estado inicial siempre `draft`.
5. **Sin AC en este paso** — se generan en `trycore:ac`. Reservar la sección "Criterios de aceptación" como placeholder con TODO.
6. **Pasa INVEST**: aunque la validación formal la hace el agente, la skill verifica las 6 letras al cierre y reporta cuáles fallan.

## Flujo de la skill

1. **Determinar siguiente ID** mirando `docs/04-historias/HU-*.md` existentes.
2. **Validar inputs** y completar con `AskUserQuestion`.
3. **Generar archivo** desde la plantilla.
4. **Auto-check INVEST**: marcar las 6 cajas que la skill puede verificar autónomamente (al menos: V, S, T). Las demás (I, N, E) son juicio del equipo.
5. **Reportar** ID asignado y los puntos INVEST a confirmar.

## Handoff

- **Agente revisor**: `invest-validator`. Hace la evaluación INVEST formal.
- **Siguiente skill**: `trycore-escribir-criterios-aceptacion-bdd` para esta misma historia.

## Ejemplos válidos (de METODOLOGIA.md)

- *"Como estudiante de la plataforma de e-learning, quiero buscar cursos por palabra clave y recibir sugerencias relevantes mientras escribo, para que pueda encontrar rápidamente el curso que necesito sin navegar todo el catálogo."*
- *"Como residente que usa la bicicleta diariamente, quiero programar reservas recurrentes para asegurar mi transporte a la hora de ir al trabajo."*

## Anti-ejemplos (rechazar)

- ✗ "Como usuario quiero el sistema rápido" → rol genérico + beneficio interno.
- ✗ "Como admin quiero que el sistema actualice automáticamente la base de datos cada hora" → es un requisito técnico, no una historia.
- ✗ "Quiero filtros" → falta rol y beneficio.
