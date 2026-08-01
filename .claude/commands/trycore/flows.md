---
name: "TRYCORE: Flows"
description: Genera flujos de navegación en Mermaid por cada épica del proyecto. Un archivo por épica bajo `docs/06-flows/EP-XXX-<slug>.md`, con trazabilidad arco↔HU↔AC. Requiere PRD + épicas + HU + Story Map ya generados.
category: Discovery
tags: [flows, navegacion, mermaid, ux, trycore]
---

Invoca el skill **trycore-mapear-flujos-navegacion**.

**Input** (opcional, tras el comando): ID de épica (`EP-XXX`) para generar solo ese flow. Si no se pasa, genera todas las épicas pendientes.

---

## Comportamiento

1. **Verificar prerequisitos**: existen `docs/01-prd/`, `docs/03-backlog/epicas.md`, `docs/04-historias/HU-*.md`, `docs/02-user-story-map/`. Si falta alguno, detener y reportar.
2. **Listar épicas** desde `epicas.md` y mapear HU asociadas vía frontmatter.
3. **Por cada épica** (o solo la indicada):
   - Decidir tipo de Mermaid (`sequenceDiagram` para interacciones actor↔sistema, `flowchart TD` para decisión ramificada).
   - Construir flow con happy + ramal de error + edge case.
   - Anotar cada arco con `%% HU-XXX`.
   - Validar cobertura: toda HU de la épica debe aparecer al menos una vez.
   - Escribir `docs/06-flows/EP-XXX-<slug>.md`.
4. **Reportar**: HU cubiertas, gaps detectados, próximo paso.

## Próximo paso

`/trycore:revisar` para que `flows-auditor` verifique trazabilidad bottom-up y top-down, y que `trazabilidad-auditor` confirme que la cadena AC→HU→Épica→Objetivo sigue intacta.
