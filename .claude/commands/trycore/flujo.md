---
name: "TRYCORE: Flujo"
description: Ejecuta el pipeline completo PRD → Épicas → Mapa → Historias con AC → Backlog → Priorización, con checkpoints entre pasos. Recupera estado si la sesión se interrumpió.
category: Workflow
tags: [pipeline, end-to-end, flujo, trycore]
---

Atajo al meta-skill **trycore-flujo-prd-a-backlog**. Invoca la skill correspondiente directamente.

**Input** (opcional, tras el comando): brief inicial del producto.

---

## Comportamiento

1. **Si existe `docs/.flow-state.json`**: leer y preguntar al usuario:
   - **Reanudar** desde el último paso completado (`last_completed_step`), o
   - **Reiniciar** el flujo (mover el archivo viejo a `docs/.flow-state.json.bak` con timestamp).

2. **Si no existe**: arrancar desde el paso 1 (PRD).

3. **Invocar el skill `trycore-flujo-prd-a-backlog`** con el brief inicial. La skill se encarga del orquestamiento, checkpoints y persistencia de estado.

4. **Reportar al final**: cuántos artefactos se generaron, dónde quedaron, qué sigue.

---

## Flujo de checkpoints (resumen)

Después de cada uno de los 6 pasos, hacer **PAUSA** mostrando:

```
✓ Paso N completado: <artefacto generado>

Resumen:
  - Archivo: <path>
  - Validaciones del agente revisor: <✓ N issues / ✗ M issues bloqueantes>

Próximo paso: <descripción>

¿Continuamos? [seguir / corregir / abortar]
```

- `seguir` → próximo paso.
- `corregir` → re-invocar la skill correspondiente para refinar el artefacto actual.
- `abortar` → guardar `.flow-state.json` y terminar con resumen ejecutivo.

---

## Guardrails

- **No avanzar si hay issues bloqueantes** del agente revisor del paso actual. Pausa siempre.
- **Persistir `.flow-state.json` después de cada paso completado**, incluso si el usuario aborta.
- Si el usuario dice "salta el mapa de historias" o similar → explicar por qué importa y permitir el override **solo con confirmación explícita**.
- Si en el paso 4 (historias) hay 10+ historias previstas, ofrecer modo "batch las primeras N sin checkpoint y después revisamos juntas".
