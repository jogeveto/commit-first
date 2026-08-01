---
id: HU-003
titulo: Emisión y validación de sesión ligada al User_ID
epica: EP-001
prioridad: Must
complejidad: M
estado: lista
openspec_change: autenticacion-identidad-linkedin-oauth2
---

# Emisión y validación de sesión ligada al User_ID

## Historia

Como **buscador de empleo autenticado**,
quiero **que cada petición que hago lleve mi identidad de forma segura**,
para **acceder solo a mis datos y mantener mi sesión sin volver a autenticarme en cada acción**.

## Contexto

Cierra EP-001: toda petición autenticada porta un User_ID verificable, que es la clave de aislamiento de EP-002. Sin esto, no hay tenancy.

## Criterios de aceptación

### Escenario 1 — Happy path: petición autenticada resuelve el User_ID
- **Dado que** tengo una sesión válida emitida tras el login
- **Cuando** llamo a un endpoint protegido con mi token de sesión
- **Entonces** el backend resuelve mi User_ID y responde con mis datos

### Escenario 2 — Error: token ausente o inválido
- **Dado que** no envío token o envío uno manipulado/expirado
- **Cuando** llamo a un endpoint protegido
- **Entonces** recibo 401 No autorizado y no se ejecuta ninguna operación

### Escenario 3 — Edge case: token expirado durante la sesión
- **Dado que** mi token de sesión ha superado su tiempo de expiración
- **Cuando** llamo a un endpoint protegido
- **Entonces** recibo 401 con indicación de re-autenticar, y ninguna respuesta expone datos

## Notas técnicas

- JWT firmado server-side (o cookie de sesión httpOnly). Claim de User_ID.
- Middleware de autorización que inyecta el User_ID en el contexto de la petición para EP-002.

## Checklist INVEST

- [x] **I**ndependent — construible tras HU-001, entrega el mecanismo de sesión
- [x] **N**egotiable — expiración y renovación son negociables
- [x] **V**aluable — habilita acceso seguro a los datos del usuario
- [x] **E**stimable — patrón estándar
- [x] **S**mall — acotada
- [x] **T**estable — AC verificables con tests de autorización
