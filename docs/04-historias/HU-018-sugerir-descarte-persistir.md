---
id: HU-018
titulo: Sugerir descarte y persistir la decisión de la vacante
epica: EP-006
prioridad: Must
complejidad: S
estado: lista
---

# Sugerir descarte y persistir la decisión de la vacante

## Historia

Como **buscador de empleo**,
quiero **que el sistema me sugiera descartar una vacante poco compatible y recuerde esa decisión**,
para **no invertir esfuerzo en oportunidades de bajo match y no volver a evaluarlas**.

## Contexto

Cierra EP-006 y el flujo end-to-end. Rama alternativa de la regla < 90%: en vez de enriquecer, el usuario puede descartar; la decisión se persiste permanentemente (Non-goal: nada efímero) y se refleja en el historial (HU-012).

## Criterios de aceptación

### Escenario 1 — Happy path: descartar persiste la decisión
- **Dado que** el match de mi CV con una vacante es bajo y el sistema sugiere descartarla
- **Cuando** confirmo el descarte
- **Entonces** la vacante queda marcada como descartada de forma permanente y no reaparece como oportunidad activa

### Escenario 2 — Error: fallo al persistir el descarte
- **Dado que** confirmo el descarte de una vacante
- **Cuando** ocurre un error al guardar la decisión
- **Entonces** el sistema me informa que no se guardó y la vacante permanece en su estado anterior (no queda en un estado ambiguo)

### Escenario 3 — Edge case: retomar una vacante descartada
- **Dado que** descarté una vacante previamente
- **Cuando** la busco en mi historial
- **Entonces** la vacante aparece en el historial con el estado "descartada" y con una acción visible para reactivarla

## Notas técnicas

- Estado `descartada` en el historial de vacantes (HU-012), con marca de tiempo.
- Persistencia permanente vía capa multi-tenant.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-016; entrega la rama de descarte
- [x] **N**egotiable — reglas de reactivación negociables
- [x] **V**aluable — el usuario no pierde tiempo en vacantes de bajo match
- [x] **E**stimable — pequeña
- [x] **S**mall — pequeña
- [x] **T**estable — AC verificables
