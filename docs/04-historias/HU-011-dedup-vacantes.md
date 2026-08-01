---
id: HU-011
titulo: Deduplicar vacantes ya vistas por el usuario
epica: EP-004
prioridad: Must
complejidad: M
estado: lista
---

# Deduplicar vacantes ya vistas por el usuario

## Historia

Como **buscador de empleo**,
quiero **que el sistema no me muestre vacantes que ya vi antes**,
para **enfocarme solo en oportunidades nuevas y no perder tiempo repitiendo revisiones**.

## Contexto

Segunda historia de EP-004. Guarda `Vacante_ID × User_ID` para filtrar de futuras búsquedas las vacantes que la persona ya vio. Es específico por usuario (no se cruza entre cuentas).

## Criterios de aceptación

### Escenario 1 — Happy path: no se repiten vacantes ya vistas
- **Dado que** ya vi ciertas vacantes en una búsqueda anterior
- **Cuando** ejecuto una nueva búsqueda que incluiría esas vacantes
- **Entonces** el sistema las excluye del resultado y solo me muestra vacantes nuevas para mí

### Escenario 2 — Error: fallo al registrar una vacante vista
- **Dado que** el sistema intenta marcar una vacante como vista
- **Cuando** ocurre un error al persistir ese registro
- **Entonces** la operación se reporta y reintenta, sin marcar como vista una vacante que no se guardó

### Escenario 3 — Edge case: la misma vacante es nueva para otro usuario
- **Dado que** yo ya vi una vacante pero otro usuario no
- **Cuando** ese otro usuario busca
- **Entonces** a él sí se le muestra la vacante (la deduplicación es por User_ID, no global)

## Notas técnicas

- Clave `(user_id, vacante_id)` única; `vacante_id` normalizado del portal de origen.
- Registrar "vista" en el momento en que la vacante se presenta al usuario.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-010; entrega la dedup
- [x] **N**egotiable — momento exacto de marcar "vista" negociable
- [x] **V**aluable — el usuario no repite ofertas
- [x] **E**stimable — acotada
- [x] **S**mall — pequeña-media
- [x] **T**estable — AC verificables multi-usuario
