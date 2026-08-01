---
name: trycore-flujo-prd-a-backlog
description: Meta-skill que orquesta el pipeline completo PRD → Épicas → Mapa de Historias → Historias con AC → Backlog → Priorización, con checkpoints de revisión entre pasos. Persiste estado en `docs/.flow-state.json` para soportar recovery si la sesión se interrumpe. Path-scope: orquesta sobre `docs/` completo. Activación: PROYECTO GREENFIELD o cuando el usuario pide "pipeline completo" / "todo el flujo" / `/trycore:flujo`; no se activa para ediciones aisladas de artefactos individuales.
category: Discovery
tags: [meta, pipeline, end-to-end, recovery, trycore]
---

# trycore-flujo-prd-a-backlog

Ejecuta el pipeline documental completo, paso a paso, con checkpoints donde el usuario aprueba o corrige antes de continuar.

## Contexto

Esta es la skill **meta** que invoca a las otras en orden. No produce artefactos directamente — encadena las que sí lo hacen.

Pipeline:

```
1. trycore-escribir-prd                        → docs/01-prd/<slug>.md
2. trycore-descomponer-prd-a-epicas            → docs/03-backlog/epicas.md
3. trycore-crear-mapa-historias                → docs/02-user-story-map/<slug>.md
4. (loop) trycore-escribir-historia-usuario
       + trycore-escribir-criterios-aceptacion-bdd  → docs/04-historias/HU-*.md
5. trycore-construir-backlog                   → docs/03-backlog/backlog.md
6. trycore-priorizar-backlog                   → docs/05-priorizacion/...
7. trycore-mapear-flujos-navegacion (opcional) → docs/06-flows/EP-*.md
```

## Persistencia y recovery

La skill escribe `docs/.flow-state.json` después de cada paso completado:

```json
{
  "last_completed_step": "epicas",
  "params": {"slug": "reservas-canchas", "framework": "moscow"},
  "timestamp": "2026-05-15T14:30:00Z",
  "history_pending": ["HU-003", "HU-004", "HU-005"]
}
```

Si se re-invoca la skill y existe ese archivo, primero pregunta al usuario: **reanudar** desde el último paso completado, o **reiniciar**.

## Checkpoints

Después de cada paso:

1. Muestra el artefacto recién generado (resumen + path).
2. Si hay reporte del agente revisor disponible (en `docs/.reviews/` o invocado en-line), lo resume.
3. Pregunta al usuario: **[seguir / corregir / abortar]**.
4. `seguir` → próximo paso. `corregir` → re-invoca la skill del paso. `abortar` → guarda estado y termina con resumen.

## Inputs requeridos

- Brief del producto (al inicio). Si no, preguntar.
- Framework de priorización (al inicio o cuando llegue al paso 6). Tomar de memory `project` si existe.
- Cuántas historias generar por épica (default: dejar que la skill infiera del Story Map; típicamente 3-5 por épica para un MVP).

## Reglas duras

1. **No saltar pasos**. Aunque el usuario diga "salta el mapa de historias", explicar por qué importa y permitir el override solo con confirmación.
2. **Cada paso pasa su quality gate** antes del siguiente: en el flujo se invoca al agente revisor correspondiente automáticamente (independiente de si los hooks PostToolUse están activos).
3. **Stop on bloqueante**: si un agente reporta issue bloqueante (ej. PRD sin objetivos medibles), pausar hasta que se corrija. No avanzar a épicas con PRD inválido.
4. **Estado siempre persistido** — no terminar el flujo sin escribir `.flow-state.json`.

## Flujo de la skill

1. **Detectar estado previo**: si existe `.flow-state.json`, preguntar reanudar / reiniciar.
2. **Recopilar inputs iniciales** (brief, framework si aplica).
3. **Ejecutar paso 1** (PRD) → checkpoint → si seguir, escribir state y paso 2.
4. **Paso 2** (épicas) → checkpoint → state → paso 3.
5. **Paso 3** (mapa) → checkpoint → state → paso 4.
6. **Paso 4** (loop por historias del mapa). Cada historia es un sub-checkpoint pero más rápido (el usuario puede decir "batch las primeras 5 sin pausa, después revisamos"). Las historias se generan con `trycore-escribir-historia-usuario` y se completan con `trycore-escribir-criterios-aceptacion-bdd`.
7. **Paso 5** (backlog consolidado) → checkpoint.
8. **Paso 6** (priorización con framework) → checkpoint.
9. **Paso 7 (opcional)** (flows de navegación con `trycore-mapear-flujos-navegacion`). Preguntar al usuario si quiere generarlos ahora; si responde sí, despachar la skill. Si responde no, saltar sin marcar gap (el artefacto es opcional). → checkpoint final.
10. **Resumen ejecutivo**: cuántos artefactos, gaps remanentes, próximos pasos sugeridos.
11. **Limpiar `.flow-state.json`** (o marcar como `last_completed_step: done`).

## Handoff

- Tras completar: sugerir `/trycore:revisar` para auditoría final antes de entregar al cliente.
- Si el usuario aborta: dejar el estado para reanudar después.

## Anti-pattern

- Saltar el mapa de historias → se pierde el journey y el MVP queda mal definido.
- Generar 50 historias sin priorizar → el equipo no sabe qué hacer primero.
- No usar checkpoints → el usuario solo ve el resultado al final, sin oportunidad de corregir.
