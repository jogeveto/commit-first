---
name: "TRYCORE: INVEST"
description: Valida una o varias historias contra los 6 criterios INVEST. Reporta ✓/✗ por letra con razonamiento y propuesta de fix. Puede invocar al agente invest-validator para segunda opinión.
category: Quality
tags: [invest, validacion, calidad, trycore]
---

Invoca el skill **trycore-validar-invest**.

**Input** (opcional): ID de historia(s) o "all" para todas. Si no se pasa, listar historias en estado `draft` y preguntar.

---

## Comportamiento

1. **Determinar historias** a validar (1, varias o todas).
2. **Para cada historia**, evaluar las 6 letras con razonamiento explícito:
   - **I**ndependent — ¿depende de otra historia para entregar valor?
   - **N**egotiable — ¿el "cómo" tiene espacio de discusión?
   - **V**aluable — ¿el beneficio es externo y visible?
   - **E**stimable — ¿el equipo puede estimar?
   - **S**mall — ¿cabe en 1 sprint?
   - **T**estable — ¿hay AC en G/W/T?
3. **Generar tabla 6×N** con ✓/✗ y propuesta de fix por ✗.
4. **Si hay > 5 historias o ambigüedad técnica** → despachar agente `invest-validator` para segunda opinión.
5. **Sugerir** marcar `estado: lista` solo en las que pasaron las 6.

## Anti-pattern a evitar

Marcar todo ✓ sin razonamiento. La skill **siempre** muestra el razonamiento detrás de cada celda.
