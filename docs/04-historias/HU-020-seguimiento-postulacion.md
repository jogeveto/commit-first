---
id: HU-020
titulo: Marcar y hacer seguimiento de la postulación
epica: EP-004
prioridad: Must
complejidad: S
estado: lista
---

# Marcar y hacer seguimiento de la postulación

## Historia

Como **buscador de empleo**,
quiero **marcar las vacantes a las que ya me postulé y ver cuáles tengo pendientes por enviar**,
para **no olvidar postulaciones ni enviar la misma dos veces**.

## Contexto

Cierra un vacío del journey: hoy el historial (HU-012) muestra qué se scrapeó y qué CV se generó, pero no si el usuario **ya envió** su postulación. El envío es manual en el portal externo (Non-goal: no auto-aplicamos); esta historia solo **registra** ese hecho y da visibilidad de lo pendiente. Contribuye a O1 (eficiencia: no reprocesar ni reenviar). Extiende el estado de vacante de EP-004 con `postulada`.

## Criterios de aceptación

### Escenario 1 — Happy path: marcar una vacante como postulada
- **Dado que** tengo una vacante en estado "cv_generado" sin postulación registrada
- **Cuando** hago clic en "Marcar como postulada" en esa vacante
- **Entonces** la vacante muestra el estado "postulada" con la fecha actual
- **Y** el contador "por postular" disminuye en uno

### Escenario 2 — Error: fallo al persistir el estado de postulación
- **Dado que** tengo una vacante en estado "cv_generado" y el servicio de persistencia falla al guardar
- **Cuando** hago clic en "Marcar como postulada"
- **Entonces** veo un mensaje de error indicando que el cambio no se guardó
- **Y** la vacante permanece en estado "cv_generado"

### Escenario 3 — Edge case: una vacante ya postulada no se ofrece como pendiente
- **Dado que** tengo una vacante en estado "postulada"
- **Cuando** abro la vista de seguimiento de postulaciones
- **Entonces** la vacante aparece en el grupo "Postuladas"
- **Y** no aparece en el grupo "Por postular"

### Escenario 4 — Edge case: conteo de pendientes por enviar
- **Dado que** tengo vacantes en estado "cv_generado" sin postular y otras en estado "postulada"
- **Cuando** abro la vista de seguimiento de postulaciones
- **Entonces** veo el conteo de "por postular" (CV listo, no enviado) separado del conteo de "postuladas"

## Notas técnicas

- Extiende el enum de estado de vacante de HU-012: `vista | cv_generado | postulada | descartada`, con `postulada_at`.
- Persistencia permanente vía capa multi-tenant (EP-002); el seguimiento es por `User_ID`.
- **No** dispara ninguna acción en el portal externo — es registro manual del usuario (respeta el Non-goal de auto-aplicar).

## Checklist INVEST

- [x] **I**ndependent — extiende el historial (HU-012); entrega el seguimiento por sí sola
- [x] **N**egotiable — la forma de marcar (botón, gesto) es negociable
- [x] **V**aluable — el usuario no olvida ni duplica postulaciones
- [x] **E**stimable — cambio de estado + vista, acotado
- [x] **S**mall — pequeña (S)
- [x] **T**estable — AC verificables (estado persistido, conteos)
