---
id: HU-007
titulo: Extraer perfil base desde LinkedIn al iniciar sesión
epica: EP-003
prioridad: Must
complejidad: M
estado: lista
---

# Extraer perfil base desde LinkedIn al iniciar sesión

## Historia

Como **buscador de empleo**,
quiero **que mi perfil profesional se construya automáticamente desde LinkedIn al entrar**,
para **tener mi información base lista sin capturarla a mano**.

## Contexto

Primera historia de valor de EP-003. Aprovecha la autorización de HU-001 para traer el perfil de LinkedIn (nombre, experiencia, educación, habilidades) y crear la base del perfil. Cubre O5.

## Criterios de aceptación

### Escenario 1 — Happy path: perfil autopoblado desde LinkedIn
- **Dado que** inicié sesión con LinkedIn y aún no tengo perfil
- **Cuando** el sistema completa la autenticación
- **Entonces** extrae los campos autorizados por el scope OAuth2 y crea mi perfil base

### Escenario 2 — Error: LinkedIn no devuelve los datos del perfil
- **Dado que** inicié sesión pero LinkedIn no expone o falla al entregar los datos del perfil
- **Cuando** el sistema intenta extraerlos
- **Entonces** mi cuenta queda activa con un perfil base vacío
- **Y** el sistema muestra un aviso de que puedo completarlo manualmente o subiendo un PDF

### Escenario 3 — Edge case: campos parciales en LinkedIn
- **Dado que** mi perfil de LinkedIn solo tiene algunos de los campos mínimos
- **Cuando** el sistema extrae la información
- **Entonces** persiste los campos disponibles y marca los faltantes como pendientes de completar

## Notas técnicas

- Alcance de la extracción limitado a lo que autoriza el scope de OAuth2 de LinkedIn.
- Persistir vía la capa multi-tenant (EP-002).
- **Campos mínimos objetivo** (negociables, tabla en discovery): nombre, experiencia, educación, habilidades. La lista exacta se acuerda con el equipo; el AC no la fija para no romperse si se omite uno.
- **Spike técnico (1-2 días)**: validar qué campos son realmente alcanzables con el scope de LinkedIn disponible (su API es restrictiva y cambia sin aviso) antes de comprometer la estimación. Documentar el resultado aquí.

## Checklist INVEST

- [x] **I**ndependent — depende de EP-001/EP-002; entrega el perfil base
- [x] **N**egotiable — mapeo de campos negociable
- [x] **V**aluable — el usuario tiene perfil sin captura manual
- [x] **E**stimable — acotada a la extracción
- [x] **S**mall — cabe en pocos días
- [x] **T**estable — AC verificables
