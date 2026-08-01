---
name: trycore-crear-mapa-historias
description: Crea un User Story Map estilo Jeff Patton con backbone cronológico (eje X), ranking por prioridad (eje Y) y líneas de release. Úsalo después de tener épicas para visualizar el journey y decidir el MVP. Path-scope principal: `docs/02-user-story-map/`. Fase del pipeline: 3 de 7 (obligatoria).
category: Discovery
tags: [story-map, jeff-patton, mvp, journey, trycore]
---

# trycore-crear-mapa-historias

Produce `docs/02-user-story-map/<slug>.md` con el mapa visual de historias organizado por journey del usuario.

## Contexto (fuente: METODOLOGIA.md §5)

Técnica de Jeff Patton. Evita el "backlog plano" donde se pierde la narrativa. Tres elementos:

1. **Backbone (eje X)** — actividades del usuario en orden cronológico.
2. **Eje Y** — historias bajo cada actividad, ordenadas por prioridad.
3. **Líneas de release** — cortes horizontales que delimitan MVP, v1.1, v2.

## Inputs requeridos

- PRD del proyecto (para extraer journey).
- `epicas.md` ya generado.
- (Opcional) Lista preliminar de historias si ya existen.

## Plantilla

`templates/artefactos/mapa-historias.template.md`.

## Reglas duras

1. **Backbone cubre el journey completo** — desde la primera interacción hasta el objetivo final del usuario. Sin saltos.
2. **Cada columna del backbone tiene ≥ 1 historia** en cualquier release. Una columna vacía indica que esa actividad no tiene cobertura.
3. **Línea de MVP explícita** — debe existir una línea de corte clara. Sin MVP definido, el mapa no entrega valor.
4. **Historias se referencian por ID** (`HU-XXX`) — si las historias aún no existen, el mapa las anticipa con IDs reservados.

## Flujo de la skill

1. **Leer PRD** y extraer el journey del usuario primario (de la sección 3 o de "Componentes principales y sitemap").
2. **Construir backbone**: 3-7 actividades en orden cronológico. Si son más, agrupar.
3. **Asignar historias existentes** a cada columna; donde falten, reservar IDs y marcar como TODO.
4. **Definir línea de MVP** explicando por qué (qué es lo mínimo para que el usuario complete el journey).
5. **Detectar gaps**: columnas con poca cobertura, actividades sin historias críticas.
6. **Escribir** `docs/02-user-story-map/<slug>.md`.

## Handoff

- **Agente revisor**: `mapping-coherence-auditor`. Verifica que el mapa cubre el journey del PRD.
- **Siguiente skill**: `trycore-escribir-historia-usuario` para cada historia anticipada en el mapa.

## Formato preferido

Tabla Markdown (más portable que mermaid para este caso). Una columna por actividad del backbone, filas agrupadas por release con un divisor visual entre releases.
