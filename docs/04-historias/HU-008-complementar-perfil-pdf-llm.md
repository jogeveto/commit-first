---
id: HU-008
titulo: Complementar el perfil subiendo un PDF (extracción por LLM local)
epica: EP-003
prioridad: Should
complejidad: L
estado: lista
---

# Complementar el perfil subiendo un PDF (extracción por LLM local)

## Historia

Como **buscador de empleo con información no reflejada en LinkedIn**,
quiero **subir un PDF con mi hoja de vida para que el sistema extraiga lo que falta**,
para **completar mi perfil sin llenar formularios largos**.

## Contexto

Único diferible a v1.1 (el journey ya cierra con HU-007). El LLM local extrae en crudo la info faltante del PDF y la fusiona con el perfil base. Complementa O5 sin costo de IA (O4).

## Criterios de aceptación

### Escenario 1 — Happy path: el PDF completa campos faltantes
- **Dado que** tengo un perfil base con campos pendientes
- **Cuando** subo un PDF con mi hoja de vida
- **Entonces** el LLM local extrae la información y completa los campos faltantes de mi perfil, dejándolos persistidos

### Escenario 2 — Error: archivo no es un PDF válido o está corrupto
- **Dado que** intento subir un archivo que no es un PDF legible
- **Cuando** el sistema intenta procesarlo
- **Entonces** rechaza el archivo con un mensaje claro y no altera mi perfil existente

### Escenario 3 — Edge case: el PDF contradice datos ya existentes
- **Dado que** el PDF trae un valor distinto para un campo que ya tengo poblado
- **Cuando** el LLM local procesa el documento
- **Entonces** el sistema muestra una notificación de conflicto con ambos valores y solicita que yo elija cuál conservar

### Escenario 4 — Edge case: PDF válido sin información aprovechable
- **Dado que** subo un PDF legible que no contiene datos de hoja de vida (p. ej. un certificado)
- **Cuando** el LLM local lo procesa
- **Entonces** el sistema informa que no encontró información para completar el perfil y lo deja sin cambios

## Notas técnicas

- LLM local (Llama/Mistral) — sin llamadas a APIs de IA de pago (O4).
- Parsing de PDF + prompt de extracción (los prompts viven en BD, ver HU-015).
- Procesamiento potencialmente largo → considerar feedback de progreso.
- **Spike prerequisito**: validar que el LLM local corre de forma estable en el entorno objetivo antes de estimar (comparte spike con HU-013).
- **Slice recomendado para acotar el sprint**: construir en dos pasos verticales — (a) upload + extracción cruda del PDF que muestra el resultado al usuario; (b) fusión con el perfil + detección de conflictos (Escenario 3). Cada paso entrega algo observable. Diferida a v1.1, no bloquea el MVP.

## Checklist INVEST

- [x] **I**ndependent — mejora incremental sobre HU-007; diferible
- [x] **N**egotiable — estrategia de fusión/conflictos negociable
- [x] **V**aluable — el usuario completa su perfil sin formularios
- [x] **E**stimable — mayor incertidumbre (LLM) pero estimable como L
- [x] **S**mall — al límite; acotada a la ingesta por PDF
- [x] **T**estable — AC verificables con PDFs de prueba
