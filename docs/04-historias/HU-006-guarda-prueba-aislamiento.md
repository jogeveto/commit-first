---
id: HU-006
titulo: Guarda de autorización y prueba de aislamiento entre usuarios
epica: EP-002
prioridad: Must
complejidad: M
estado: lista
---

# Guarda de autorización y prueba de aislamiento entre usuarios

## Historia

Como **usuario dueño de mis datos**,
quiero **que exista una verificación automática de que ningún usuario puede leer datos de otro**,
para **confiar en que mi privacidad está garantizada y no depende del cuidado manual del desarrollador**.

## Contexto

Cierra EP-002. Añade la guarda de autorización a nivel de operación y una prueba automatizada de aislamiento que cumple el KPI de O3 (fugas = 0), exigido en cada release.

## Criterios de aceptación

### Escenario 1 — Happy path: la prueba de aislamiento pasa
- **Dado que** existen dos usuarios (A y B) con datos propios
- **Cuando** se ejecuta la prueba automatizada de aislamiento
- **Entonces** confirma que A no puede leer ni modificar ningún registro de B, y viceversa

### Escenario 2 — Error: operación de escritura sobre un recurso ajeno es rechazada
- **Dado que** el usuario A está autenticado
- **Cuando** intenta modificar o borrar un registro que pertenece a B
- **Entonces** la guarda de autorización responde 403/404 y no altera el dato de B

### Escenario 3 — Edge case: petición sin User_ID en contexto se bloquea
- **Dado que** una petición llega a la capa de datos sin User_ID resuelto en el contexto
- **Cuando** intenta acceder a datos de dominio
- **Entonces** la guarda la rechaza en lugar de ejecutar una consulta sin filtro de tenant

## Notas técnicas

- Prueba de autorización cross-tenant como parte de la suite; corre en el Release Gate.
- La guarda debe fallar-cerrado: sin User_ID → denegar.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-004/005; entrega la garantía verificable
- [x] **N**egotiable — forma de la prueba negociable
- [x] **V**aluable — convierte el aislamiento en una garantía comprobable
- [x] **E**stimable — acotada
- [x] **S**mall — cabe en pocos días
- [x] **T**estable — es, en sí misma, una prueba
