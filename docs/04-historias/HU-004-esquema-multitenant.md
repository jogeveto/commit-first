---
id: HU-004
titulo: Esquema de datos multi-tenant con User_ID en toda entidad
epica: EP-002
prioridad: Must
complejidad: M
estado: lista
---

# Esquema de datos multi-tenant con User_ID en toda entidad

## Historia

Como **usuario dueño de mis datos**,
quiero **que cada registro que genero quede marcado como mío en la base de datos**,
para **tener la garantía de que mis datos existen separados de los de cualquier otra persona**.

## Contexto

Cimiento transversal de EP-002. Define el esquema PostgreSQL donde toda entidad (perfil, vacante, CV, match, prompt, historial) lleva `User_ID`. Base de O3 (aislamiento 100%).

## Criterios de aceptación

### Escenario 1 — Happy path: entidad se crea con el User_ID del dueño
- **Dado que** existe un usuario autenticado con su User_ID
- **Cuando** se crea un registro de dominio (perfil, vacante, CV, etc.)
- **Entonces** el registro se persiste con la columna User_ID apuntando a ese dueño

### Escenario 2 — Error: intento de crear una entidad de dominio sin User_ID
- **Dado que** una operación intenta insertar un registro de dominio sin User_ID
- **Cuando** se ejecuta la escritura en la base de datos
- **Entonces** la restricción NOT NULL / FK rechaza el insert y no queda registro huérfano

### Escenario 3 — Edge case: borrado del usuario arrastra sus registros
- **Dado que** un usuario tiene registros de dominio asociados
- **Cuando** se elimina la cuenta del usuario
- **Entonces** sus registros se eliminan o quedan íntegros según la política de FK definida, sin dejar registros apuntando a un User_ID inexistente

## Notas técnicas

- Columna `user_id` NOT NULL + FK a la tabla de usuarios en toda tabla de dominio. Índice por `user_id`.
- Migraciones versionadas.
- **Valor observable**: esta historia es cimiento (entrega esquema + constraints). La garantía de aislamiento se vuelve observable por test de integración en **HU-006** (prueba de autorización cross-tenant); los escenarios de aquí se validan con tests de esquema/constraints ejecutables, no solo por inspección del DDL.

## Checklist INVEST

- [x] **I**ndependent — cimiento; se construye tras EP-001
- [x] **N**egotiable — política de borrado (cascade vs restrict) negociable
- [x] **V**aluable — habilita la garantía de aislamiento que el usuario exige
- [x] **E**stimable — modelado de datos estándar
- [x] **S**mall — acotada al esquema
- [x] **T**estable — verificable con constraints y tests de esquema
