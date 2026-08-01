---
id: HU-017
titulo: Ofrecer formulario de enriquecimiento cuando el match < 90%
epica: EP-006
prioridad: Must
complejidad: M
estado: lista
---

# Ofrecer formulario de enriquecimiento cuando el match < 90%

## Historia

Como **buscador de empleo**,
quiero **que cuando mi match sea bajo el sistema me pida la información que falta**,
para **mejorar mi CV y aumentar mi coincidencia con la vacante**.

## Contexto

Implementa la regla de negocio central de O2: si el match < 90%, se genera un formulario React con los datos faltantes detectados; al completarlo, se regenera/reaudita el CV. El umbral 90% es la regla dura del PRD.

## Criterios de aceptación

### Escenario 1 — Happy path: enriquecer sube el match
- **Dado que** el match de mi CV con la vacante es menor a 90% y el formulario de enriquecimiento está visible
- **Cuando** completo el formulario con la información faltante y lo envío
- **Entonces** el CV se regenera con los nuevos datos
- **Y** la nueva auditoría ATS muestra un porcentaje de match actualizado

### Escenario 2 — Error: envío el formulario con datos inválidos o vacíos
- **Dado que** el formulario de enriquecimiento está abierto
- **Cuando** lo envío sin completar los campos requeridos o con datos inválidos
- **Entonces** el sistema valida y me indica qué corregir, sin regenerar el CV con datos incompletos

### Escenario 3 — Edge case: match ≥ 90% no dispara el formulario
- **Dado que** el match de mi CV ya es 90% o más
- **Cuando** termina la auditoría
- **Entonces** el sistema no me muestra el formulario de enriquecimiento y me indica que el CV está listo para postular

## Notas técnicas

- Formulario React generado a partir de los campos faltantes que reporta el auditor (HU-016).
- Umbral 90% configurable pero por defecto el del PRD.
- Al enriquecer, encadena con HU-013 (regenerar) + HU-016 (reauditar).

## Checklist INVEST

- [x] **I**ndependent — depende de HU-016; entrega la regla <90% (rama enriquecer)
- [x] **N**egotiable — UX del formulario negociable
- [x] **V**aluable — el usuario mejora su match
- [x] **E**stimable — acotada
- [x] **S**mall — media
- [x] **T**estable — AC verificables con casos por debajo/encima del umbral
