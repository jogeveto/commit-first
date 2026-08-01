---
id: HU-021
titulo: Cerrar sesión
epica: EP-001
prioridad: Must
complejidad: S
estado: lista
openspec_change: autenticacion-identidad-linkedin-oauth2
---

# Cerrar sesión

## Historia

Como **buscador de empleo que comparte el equipo con otra persona**,
quiero **cerrar mi sesión cuando termino de usar el sistema**,
para **que quien use el computador después no vea ni modifique mi información**.

## Contexto

Contraparte natural de HU-003 (sesión ligada al `User_ID`). El uso previsto es familiar y cerrado: dos personas (p. ej. Ana y Diego) con cuentas totalmente independientes que comparten dispositivo. Sin una salida explícita, la sesión de quien entró primero sigue activa y el aislamiento por tenant queda anulado en la práctica, por muy correcto que sea en el servidor.

El prototipo aprobado ya contempla la acción (botón "Salir" en la barra superior, `docs/07-prototipo/index.html`). Esta historia formaliza el requisito que ese elemento venía a satisfacer.

## Criterios de aceptación

### Escenario 1 — Happy path: cerrar sesión devuelve al login
- **Dado que** tengo una sesión activa y estoy en mi área autenticada
- **Cuando** elijo la opción de salir
- **Entonces** mi sesión se descarta del navegador y vuelvo a la pantalla de login

### Escenario 2 — Error: intentar volver al área autenticada tras salir
- **Dado que** acabo de cerrar sesión
- **Cuando** intento abrir directamente una ruta del área autenticada
- **Entonces** el sistema me devuelve al login y no muestra ningún dato de mi cuenta

### Escenario 3 — Edge case: la sesión no sobrevive al cierre de la pestaña
- **Dado que** tengo una sesión activa
- **Cuando** cierro la pestaña del navegador y vuelvo a abrir la aplicación
- **Entonces** no hay sesión activa y se me pide entrar de nuevo

## Notas técnicas

- El descarte es del lado del cliente (la sesión vive en `sessionStorage`); el JWT es sin estado, así que no hay sesión de servidor que invalidar. Una eventual lista de revocación de tokens queda fuera del alcance de esta historia.
- El escenario 3 se cumple por construcción al usar `sessionStorage` en lugar de `localStorage`.
- La autoridad real del aislamiento sigue siendo el backend: el guard del SPA es comodidad de navegación, no el control de acceso (HU-003).

## Checklist INVEST

- [x] **I**ndependent — se entrega sobre HU-003 pero es una capacidad separable
- [x] **N**egotiable — el lugar del control y el destino tras salir son negociables
- [x] **V**aluable — protege la privacidad entre las personas que comparten el equipo
- [x] **E**stimable — descarte de sesión y redirección; trivialmente estimable
- [x] **S**mall — muy pequeña
- [x] **T**estable — los 3 escenarios son verificables (tests de sesión + navegación real)
