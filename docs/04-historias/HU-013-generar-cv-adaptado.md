---
id: HU-013
titulo: Generar un CV adaptado a las palabras clave de la vacante
epica: EP-005
prioridad: Must
complejidad: L
estado: lista
---

# Generar un CV adaptado a las palabras clave de la vacante

## Historia

Como **buscador de empleo**,
quiero **generar una hoja de vida adaptada a una vacante específica**,
para **maximizar la coincidencia con las palabras clave que evalúa el filtro ATS**.

## Contexto

Corazón de EP-005. El LLM local combina mi perfil (EP-003) con la descripción de la vacante (EP-004) y produce un CV alineado a sus palabras clave. Cubre O1 y O4 (LLM local, costo $0).

## Criterios de aceptación

### Escenario 1 — Happy path: se genera un CV adaptado
- **Dado que** tengo un perfil poblado y he seleccionado una vacante
- **Cuando** solicito generar el CV
- **Entonces** el LLM local produce un CV que incluye al menos el umbral configurado de las palabras clave presentes en la descripción de la vacante, disponible para revisar

### Escenario 2 — Error: falta información base para generar
- **Dado que** mi perfil no tiene la información mínima necesaria
- **Cuando** solicito generar el CV
- **Entonces** el sistema me indica qué falta en el perfil en lugar de generar un CV vacío o inválido

### Escenario 3 — Edge case: la generación del LLM local falla o excede el tiempo
- **Dado que** el modelo local no está disponible o tarda más del límite aceptable
- **Cuando** solicito generar el CV
- **Entonces** recibo un error controlado con opción de reintentar, sin dejar un CV a medias persistido

### Escenario 4 — Performance: generación dentro del presupuesto de tiempo
- **Dado que** tengo un perfil poblado y una vacante seleccionada
- **Cuando** el LLM local completa la generación del CV
- **Entonces** el CV está disponible dentro del presupuesto de tiempo definido para el paso de generación (parte del objetivo end-to-end < 5 min, O1)

## Notas técnicas

- LLM local (Llama/Mistral) — sin APIs de IA de pago (O4).
- Prompt de generación tomado del gestor de prompts en BD (HU-015).
- Objetivo end-to-end (generación + auditoría) < 5 min (O1).
- **Spike previo**: medir la latencia real del modelo local elegido en el entorno objetivo antes de comprometer la estimación L.
- El "umbral configurado" de palabras clave del Escenario 1 se fija como parámetro (p. ej. ≥ N de las palabras clave de la vacante) para hacer el AC verificable.

## Checklist INVEST

- [x] **I**ndependent — depende de EP-003/EP-004; entrega la generación
- [x] **N**egotiable — estrategia de prompting negociable
- [x] **V**aluable — el usuario obtiene un CV adaptado
- [x] **E**stimable — incertidumbre por LLM, estimable como L
- [x] **S**mall — acotada a la generación (export es HU-014)
- [x] **T**estable — AC verificables
