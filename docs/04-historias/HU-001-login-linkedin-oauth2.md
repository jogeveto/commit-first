---
id: HU-001
titulo: Iniciar sesión con LinkedIn (OAuth2)
epica: EP-001
prioridad: Must
complejidad: M
estado: lista
---

# Iniciar sesión con LinkedIn (OAuth2)

## Historia

Como **buscador de empleo**,
quiero **iniciar sesión usando mi cuenta de LinkedIn**,
para **entrar al sistema sin crear ni recordar contraseñas**.

## Contexto

Es la única puerta de entrada al sistema (Non-goal: no hay registro manual ni contraseñas). Atraviesa la actividad "Entrar" del journey y es cimiento de EP-001.

## Criterios de aceptación

### Escenario 1 — Happy path: autenticación exitosa
- **Dado que** estoy en la pantalla de login y no tengo sesión activa
- **Cuando** autorizo el acceso en el flujo OAuth2 de LinkedIn
- **Entonces** el sistema me redirige autenticado a mi área y emite una sesión ligada a mi User_ID

### Escenario 2 — Error: el usuario cancela el consentimiento en LinkedIn
- **Dado que** inicié el flujo OAuth2 de LinkedIn
- **Cuando** rechazo el consentimiento o LinkedIn devuelve un error de autorización
- **Entonces** vuelvo a la pantalla de login con un mensaje claro de que la autenticación no se completó y sin sesión creada

### Escenario 3 — Edge case: callback con state inválido (protección CSRF)
- **Dado que** el sistema envió un parámetro `state` al iniciar el flujo OAuth2
- **Cuando** el callback de LinkedIn llega con un `state` ausente o que no coincide
- **Entonces** el sistema rechaza la autenticación, no crea sesión y registra el intento

## Notas técnicas

- OAuth2 Authorization Code flow contra LinkedIn. Client secret solo server-side (nunca en el frontend React).
- Validar `state` para CSRF; validar `redirect_uri`.

## Checklist INVEST

- [x] **I**ndependent — es la base; no depende de otras historias para entregarse
- [x] **N**egotiable — el mecanismo de sesión (JWT vs cookie) es negociable
- [x] **V**aluable — el usuario entra sin fricción de registro
- [x] **E**stimable — flujo OAuth2 estándar, estimable
- [x] **S**mall — cabe en pocos días
- [x] **T**estable — AC verificables como tests de integración
