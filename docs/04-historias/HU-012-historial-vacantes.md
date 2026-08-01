---
id: HU-012
titulo: Persistir y consultar el historial de vacantes
epica: EP-004
prioridad: Must
complejidad: S
estado: lista
---

# Persistir y consultar el historial de vacantes

## Historia

Como **buscador de empleo**,
quiero **consultar el historial de vacantes que he visto y trabajado**,
para **retomar oportunidades y llevar el control de mi búsqueda a lo largo del tiempo**.

## Contexto

Cierra EP-004. El historial de vacantes por usuario se persiste de forma permanente (Non-goal: nada efímero) y alimenta la generación de CV (EP-005) y las decisiones de descarte (EP-006).

## Criterios de aceptación

### Escenario 1 — Happy path: consulto mi historial
- **Dado que** he visto vacantes en búsquedas anteriores
- **Cuando** abro mi historial de vacantes
- **Entonces** veo la lista persistida de vacantes con su estado (vista, con CV generado, descartada)

### Escenario 2 — Error: consulta con filtro inválido
- **Dado que** estoy en mi historial de vacantes
- **Cuando** aplico un filtro no soportado
- **Entonces** el sistema reporta que el filtro no es válido y mantiene visible el historial completo, sin devolver un error de servidor

### Escenario 3 — Edge case: historial vacío para usuario nuevo
- **Dado que** soy un usuario que aún no ha buscado vacantes
- **Cuando** abro mi historial
- **Entonces** veo un estado vacío informativo, nunca vacantes de otro usuario

## Notas técnicas

- Estado de la vacante como enum (vista / cv_generado / descartada) para trazar el flujo.
- Persistencia permanente vía capa multi-tenant.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-010/011; entrega consulta del historial
- [x] **N**egotiable — atributos del historial negociables
- [x] **V**aluable — el usuario controla su búsqueda en el tiempo
- [x] **E**stimable — CRUD simple
- [x] **S**mall — pequeña
- [x] **T**estable — AC verificables
