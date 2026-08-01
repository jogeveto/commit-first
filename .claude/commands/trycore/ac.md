---
name: "TRYCORE: AC"
description: Escribe los Acceptance Criteria de una historia en formato Given/When/Then (BDD/Gherkin), incluyendo happy path, error y edge case. Reemplaza la sección AC de la historia indicada.
category: Discovery
tags: [acceptance-criteria, bdd, gherkin, trycore]
---

Invoca el skill **trycore-escribir-criterios-aceptacion-bdd**.

**Input** (opcional, tras el comando): ID de la historia (`HU-XXX`) o path al archivo. Si no se pasa, listar historias sin AC y preguntar.

---

## Comportamiento

1. **Cargar la historia** objetivo (resolver ID → path).
2. **Razonar 3-5 escenarios** que cubran happy + error + edge.
3. **Escribir cada escenario** en formato Given/When/Then estricto:
   - Given describe ESTADO.
   - When describe UNA acción.
   - Then describe resultado OBSERVABLE.
4. **Reemplazar la sección "Criterios de aceptación"** del archivo (no tocar el resto).
5. **Auto-check**: contar escenarios, marcar ambigüedades.
6. **Reportar** los escenarios escritos.

## Próximo paso

`/trycore:invest HU-XXX` ahora que la "T" (Testable) se puede verificar.
