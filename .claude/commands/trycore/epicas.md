---
name: "TRYCORE: Épicas"
description: Descompone el PRD existente en épicas (EP-XXX) con trazabilidad explícita a los objetivos del PRD. Genera docs/03-backlog/epicas.md.
category: Discovery
tags: [epicas, decomposition, trazabilidad, trycore]
---

Invoca el skill **trycore-descomponer-prd-a-epicas**.

---

## Preflight

- Verificar que existe ≥ 1 PRD en `docs/01-prd/`. Si no, redirigir a `/trycore:prd`.

## Comportamiento

1. Leer el PRD activo (si hay varios, preguntar cuál).
2. Extraer objetivos (sección 1), capabilities (sección 5), KPIs (sección 12).
3. Generar 3-7 épicas correlativas (`EP-001`, `EP-002`, …) con:
   - Título, resumen, justificación.
   - Objetivos del PRD que cubre.
   - Capabilities incluidas.
   - Métrica de éxito de la épica.
4. Matriz de trazabilidad **Épica × Objetivo-PRD** al final del archivo.
5. Escribir `docs/03-backlog/epicas.md`.
6. **Reportar huérfanos** si los hay (objetivos sin cobertura, épicas sin objetivo claro).

## Próximo paso

`/trycore:mapa` para visualizar el journey, o entrar directamente a historias con `/trycore:historia`.
