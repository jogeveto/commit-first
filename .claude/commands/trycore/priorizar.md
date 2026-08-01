---
name: "TRYCORE: Priorizar"
description: Prioriza el backlog aplicando un framework (MoSCoW, RICE, Valor-Esfuerzo o Eisenhower). Genera docs/05-priorizacion/<framework>-<fecha>.md y opcionalmente reordena backlog.md.
category: Discovery
tags: [priorizacion, moscow, rice, trycore]
---

Invoca el skill **trycore-priorizar-backlog**.

**Input** (opcional): nombre del framework. Si no se pasa, leer auto-memory `project` o preguntar.

---

## Comportamiento

1. **Determinar framework**:
   - Lee auto-memory tipo `project` → `prioritization_framework`.
   - Si no hay (no se hizo `/trycore:onboard`), preguntar con AskUserQuestion:
     - MoSCoW (Recommended) — simple, intuitivo
     - RICE — cuantitativo, requiere datos
     - Valor / Esfuerzo — matriz 2×2 rápida
     - Eisenhower — urgencia × importancia
2. **Cargar backlog** y todas las historias.
3. **Recopilar información** para aplicar el framework:
   - MoSCoW: pregunta categoría por historia (o infiere).
   - RICE: pregunta los 4 valores (Reach, Impact, Confidence, Effort).
   - Valor/Esfuerzo: alto/bajo en cada eje.
   - Eisenhower: urgente/no urgente, importante/no importante.
4. **Calcular o agrupar** según el framework.
5. **Generar** `docs/05-priorizacion/<framework>-<YYYY-MM-DD>.md`.
6. **Validar distribución**: si MoSCoW con > 60% Must, alertar. Si RICE sin señal clara, alertar.
7. **Mostrar resumen** al usuario y preguntar: ¿actualizar el orden de filas en `backlog.md` con la nueva priorización?
8. Si aprueba → reordenar y actualizar la columna "Prioridad".

## Próximo paso

- Implementación (planning de sprint).
- O `/trycore:revisar` para auditoría final antes de entregar al cliente.
