---
id: HU-010
titulo: Buscar vacantes bajo demanda en un portal
epica: EP-004
prioridad: Must
complejidad: L
estado: lista
---

# Buscar vacantes bajo demanda en un portal

## Historia

Como **buscador de empleo**,
quiero **buscar vacantes en un portal de empleo bajo demanda**,
para **encontrar oportunidades relevantes a mi perfil sin revisar cada portal a mano**.

## Contexto

Primera historia de EP-004. Consume scraping o API de un portal (LinkedIn/Computrabajo/El Empleo) y devuelve vacantes para el usuario. El motor se aísla tras una interfaz para agregar portales luego.

## Criterios de aceptación

### Escenario 1 — Happy path: la búsqueda devuelve vacantes
- **Dado que** estoy autenticado y defino un criterio de búsqueda (cargo, ubicación)
- **Cuando** ejecuto la búsqueda
- **Entonces** el sistema consulta el portal y me muestra una lista de vacantes con su información básica

### Escenario 2 — Error: el portal no responde o falla
- **Dado que** el portal externo está caído o rechaza la petición
- **Cuando** ejecuto la búsqueda
- **Entonces** recibo un mensaje de que la búsqueda no se pudo completar y puedo reintentar, sin datos corruptos guardados

### Escenario 3 — Edge case: búsqueda sin resultados
- **Dado que** mi criterio de búsqueda no coincide con ninguna vacante del portal
- **Cuando** ejecuto la búsqueda
- **Entonces** veo un estado vacío claro ("sin resultados") en lugar de un error

### Escenario 4 — Performance: la búsqueda responde a tiempo
- **Dado que** ejecuto una búsqueda válida
- **Cuando** el portal responde
- **Entonces** el sistema entrega los resultados en menos de 30 segundos (no-funcional del PRD)

## Notas técnicas

- Interfaz de "proveedor de vacantes" para poder añadir portales sin tocar el resto (mitiga riesgo externo).
- Respuesta objetivo < 30 s (no-funcional del PRD).
- Respetar términos de uso / rate limits del portal.
- **Decisión de sprint (no de implementación)**: elegir **API oficial vs scraping** según disponibilidad del portal — cambia el esfuerzo en un orden de magnitud (rate limits, selectores frágiles). Se decide en sprint planning.
- **Enfoque mock-first para acotar el sprint**: primero implementar la búsqueda end-to-end contra un portal *mockeado* (validando la interfaz de proveedor), luego conectar el primer portal real. Permite demostrar valor aunque el portal externo dé problemas.

## Checklist INVEST

- [x] **I**ndependent — depende de EP-002; entrega la búsqueda
- [x] **N**egotiable — qué portal primero es negociable
- [x] **V**aluable — el usuario encuentra vacantes
- [x] **E**stimable — incertidumbre por scraping, estimable como L
- [x] **S**mall — acotada a un portal
- [x] **T**estable — AC verificables (con portal mockeado)
