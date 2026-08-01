---
id: HU-005
titulo: Capa de acceso a datos que filtra por tenant
epica: EP-002
prioridad: Must
complejidad: M
estado: lista
---

# Capa de acceso a datos que filtra por tenant

## Historia

Como **usuario dueño de mis datos**,
quiero **que cada consulta que hace el sistema devuelva únicamente mis registros**,
para **no ver nunca información de otro usuario ni exponer la mía**.

## Contexto

Cimiento transversal de EP-002. La capa de acceso a datos aplica el filtro `WHERE user_id = <actual>` de forma sistemática, tomando el User_ID del contexto de sesión (HU-003).

## Criterios de aceptación

### Escenario 1 — Happy path: la consulta devuelve solo mis registros
- **Dado que** estoy autenticado y existen registros míos y de otros usuarios
- **Cuando** consulto una colección de dominio (mis vacantes, mis CVs)
- **Entonces** el resultado contiene exclusivamente registros con mi User_ID

### Escenario 2 — Error: acceso directo por ID a un registro ajeno
- **Dado que** conozco el identificador de un registro que pertenece a otro usuario
- **Cuando** solicito ese registro por su ID
- **Entonces** el sistema responde "no encontrado" y no revela el registro ajeno

### Escenario 3 — Edge case: colección vacía no filtra al usuario equivocado
- **Dado que** aún no tengo registros propios en una colección
- **Cuando** la consulto
- **Entonces** recibo una lista vacía y en ningún caso registros de otros usuarios

## Notas técnicas

- Filtro por tenant centralizado en el repositorio/DAL, no repetido ad-hoc en cada endpoint.
- Considerar Row-Level Security de PostgreSQL como defensa en profundidad.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-004; entrega el filtrado
- [x] **N**egotiable — implementación (DAL vs RLS) negociable
- [x] **V**aluable — el usuario nunca ve datos ajenos
- [x] **E**stimable — patrón conocido
- [x] **S**mall — acotada
- [x] **T**estable — verificable con tests multi-usuario
