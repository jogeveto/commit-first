---
id: flow-006-auditoria-ats-enriquecimiento
epica: EP-006
historias_cubiertas: [HU-016, HU-017, HU-018]
---

# Flow 006 — Auditoría ATS y Enriquecimiento

## Resumen

Actor: **Buscador de empleo**. Objetivo: auditar el CV contra la vacante (% match), y según la regla < 90% enriquecer o descartar, persistiendo toda decisión. Condición de éxito: el usuario sabe si su CV pasa el ATS y decide con la información persistida. Cierra el flujo end-to-end.

## Diagrama

```mermaid
flowchart TD
  Inicio[CV en PDF generado para una vacante] --> Audita[Solicita auditoría ATS]
  %% HU-016
  Audita --> Legible{¿PDF con texto extraíble?}
  %% HU-016
  Legible -- no --> ErrLeer[Reporta que no pudo leer el CV, sin % inventado]
  %% HU-016
  Legible -- sí --> Desc{¿Descripción de vacante suficiente?}
  %% HU-016
  Desc -- muy corta/vacía --> Baja[Devuelve % + campo confianza: baja]
  %% HU-016
  Desc -- suficiente --> Match[Devuelve % entre 0 y 100 en pantalla]
  %% HU-016
  Match --> Determinismo[Misma entrada -> mismo % en 2 corridas]

  %% HU-017
  Match --> Regla{¿Match ≥ 90%?}
  %% HU-017
  Regla -- sí --> Listo[No muestra formulario: CV listo para postular]
  %% HU-017
  Regla -- no --> Form[Muestra formulario de enriquecimiento]
  %% HU-017
  Form --> Valida{¿Datos del formulario válidos?}
  %% HU-017
  Valida -- no --> ErrForm[Indica qué corregir, no regenera con datos incompletos]
  %% HU-017
  Valida -- sí --> Regenera[Regenera CV -> nueva auditoría con match actualizado]

  %% HU-018
  Regla -- no --> Descartar[Sugiere descartar la vacante]
  %% HU-018
  Descartar --> ConfDesc{¿Confirma descarte?}
  %% HU-018
  ConfDesc -- sí --> Persiste[Marca descartada permanentemente, no reaparece activa]
  %% HU-018
  ConfDesc -- fallo al guardar --> ErrDesc[Informa que no se guardó, mantiene estado anterior]
  %% HU-018
  Persiste --> Retomar[En historial: estado descartada + acción para reactivar]
```

## Trazabilidad

| Paso | HU | AC |
|---|---|---|
| Entrega % de match en pantalla (0–100) | HU-016 | AC-1 (happy) |
| PDF ilegible → sin % inventado | HU-016 | AC-2 (error) |
| Descripción corta → confianza: baja | HU-016 | AC-3 (edge) |
| Determinismo: misma entrada, mismo % | HU-016 | AC-4 (determinismo) |
| Enriquecer sube el match | HU-017 | AC-1 (happy) |
| Formulario con datos inválidos | HU-017 | AC-2 (error) |
| Match ≥ 90% no dispara formulario | HU-017 | AC-3 (edge) |
| Descartar persiste la decisión | HU-018 | AC-1 (happy) |
| Fallo al persistir el descarte | HU-018 | AC-2 (error) |
| Retomar una vacante descartada | HU-018 | AC-3 (edge) |

## Notas

`flowchart TD` porque domina la lógica de decisión (regla < 90% con ramas enriquecer/descartar). Consume el PDF de HU-014 (EP-005). Cierra el journey end-to-end. Las 3 HU quedan cubiertas.
