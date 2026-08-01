---
id: HU-002
titulo: Provisión automática de cuenta en primer acceso
epica: EP-001
prioridad: Must
complejidad: S
estado: lista
openspec_change: autenticacion-identidad-linkedin-oauth2
---

# Provisión automática de cuenta en primer acceso

## Historia

Como **buscador de empleo que entra por primera vez**,
quiero **que mi cuenta se cree automáticamente al autenticarme con LinkedIn**,
para **empezar a usar el sistema sin llenar formularios de registro**.

## Contexto

Complementa HU-001: tras la autenticación, si el usuario no existe se provisiona su cuenta ligada a su identidad de LinkedIn. Cubre el objetivo O5 (100% de altas sin formulario manual).

## Criterios de aceptación

### Escenario 1 — Happy path: primer acceso crea la cuenta
- **Dado que** me autentico con LinkedIn y no existe una cuenta para mi identidad
- **Cuando** el sistema procesa el callback autenticado
- **Entonces** se crea mi cuenta con un User_ID nuevo y quedo con sesión activa

### Escenario 2 — Error: fallo al persistir la cuenta
- **Dado que** me autentico con LinkedIn por primera vez
- **Cuando** ocurre un error al guardar la cuenta en la base de datos
- **Entonces** no se emite sesión y veo un mensaje de que no se pudo completar el alta, sin cuenta parcial creada

### Escenario 3 — Edge case: acceso recurrente no duplica la cuenta
- **Dado que** ya tengo una cuenta provisionada con mi identidad de LinkedIn
- **Cuando** me autentico de nuevo
- **Entonces** el sistema reutiliza mi User_ID existente y no crea una cuenta duplicada

## Notas técnicas

- Identidad estable de LinkedIn (subject/`sub`) como clave natural para idempotencia del alta.
- Transacción atómica para evitar cuentas parciales.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-001 pero entrega valor propio (alta)
- [x] **N**egotiable — datos mínimos a copiar en el alta son negociables
- [x] **V**aluable — el usuario existe en el sistema sin registro manual
- [x] **E**stimable — CRUD idempotente, estimable
- [x] **S**mall — pequeña
- [x] **T**estable — AC verificables
