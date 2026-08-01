---
id: HU-009
titulo: Persistir y consultar el perfil estructurado del usuario
epica: EP-003
prioridad: Must
complejidad: S
estado: lista
---

# Persistir y consultar el perfil estructurado del usuario

## Historia

Como **buscador de empleo**,
quiero **ver y recuperar mi perfil estructurado cuando vuelvo al sistema**,
para **usarlo como base para generar mis CVs sin volver a cargarlo**.

## Contexto

Cierra EP-003. El perfil (venga de LinkedIn o del PDF) queda persistido por usuario y disponible para EP-005 (generación de CV). Es dato permanente (Non-goal: nada efímero).

## Criterios de aceptación

### Escenario 1 — Happy path: recupero mi perfil persistido
- **Dado que** ya tengo un perfil creado en una sesión anterior
- **Cuando** vuelvo a entrar y abro mi perfil
- **Entonces** veo mi información estructurada (experiencia, educación, habilidades) tal como quedó guardada

### Escenario 2 — Error: consulta de perfil inexistente
- **Dado que** soy un usuario cuyo perfil aún no se ha creado
- **Cuando** consulto mi perfil
- **Entonces** el sistema responde indicando que no hay perfil todavía, sin error de servidor

### Escenario 3 — Edge case: aislamiento del perfil entre usuarios
- **Dado que** otro usuario tiene su propio perfil
- **Cuando** consulto mi perfil
- **Entonces** solo obtengo el mío, nunca el de otro usuario (verifica EP-002)

## Notas técnicas

- Persistencia permanente vía la capa multi-tenant (EP-002).
- Modelo estructurado (secciones de CV) reutilizable por EP-005.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-007; entrega lectura/persistencia
- [x] **N**egotiable — forma exacta del modelo negociable
- [x] **V**aluable — el perfil está disponible para generar CVs
- [x] **E**stimable — CRUD simple
- [x] **S**mall — pequeña
- [x] **T**estable — AC verificables
