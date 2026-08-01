---
id: HU-016
titulo: Calcular el % de match ATS entre el CV y la vacante
epica: EP-006
prioridad: Must
complejidad: L
estado: lista
---

# Calcular el % de match ATS entre el CV y la vacante

## Historia

Como **buscador de empleo**,
quiero **ver un porcentaje exacto de coincidencia entre mi CV y la vacante**,
para **saber objetivamente si mi CV tiene posibilidades de pasar el filtro ATS**.

## Contexto

Primera historia de EP-006. Un microservicio Python con NLP compara el PDF del CV (HU-014) con la descripción de la vacante y entrega un % de match exacto. Cubre O1 (cierre) y contribuye a O2.

## Criterios de aceptación

### Escenario 1 — Happy path: se entrega un % de match
- **Dado que** tengo un CV generado (PDF) para una vacante
- **Cuando** solicito la auditoría ATS
- **Entonces** el auditor compara el CV con la vacante y me devuelve un valor numérico entre 0 y 100 (entero o con un decimal) mostrado en pantalla asociado a ese CV y esa vacante

### Escenario 2 — Error: el PDF no es legible por el auditor
- **Dado que** el CV en PDF no contiene texto extraíble (ej. es una imagen)
- **Cuando** el auditor intenta analizarlo
- **Entonces** reporta que no pudo leer el CV y no entrega un porcentaje inventado

### Escenario 3 — Edge case: vacante con descripción muy corta o vacía
- **Dado que** la vacante casi no tiene texto de descripción
- **Cuando** se ejecuta la auditoría
- **Entonces** el auditor devuelve el porcentaje calculado junto con un campo `confianza: baja` (o advertencia visible) por descripción insuficiente

### Escenario 4 — Determinismo: misma entrada, mismo resultado
- **Dado que** tengo el mismo CV y la misma vacante
- **Cuando** ejecuto la auditoría ATS dos veces seguidas
- **Entonces** el porcentaje devuelto es idéntico en ambas ejecuciones (cálculo determinista)

## Notas técnicas

- Microservicio/scripts en Python con NLP (spaCy / scikit-learn u similar).
- Requiere PDF con texto seleccionable (dependencia con HU-014).
- Cálculo **determinista** (misma entrada → mismo %), no delegado al LLM.
- **Enfoque mock-first para acotar el sprint**: primero levantar el microservicio Python (endpoint vivo que acepta texto y devuelve un número, aunque sea TF-IDF simple) y luego refinar el algoritmo NLP con los edge cases. Reduce el riesgo de infraestructura (runtime Python nuevo) separándolo del algoritmo.

## Checklist INVEST

- [x] **I**ndependent — depende de EP-005; entrega el cálculo de match
- [x] **N**egotiable — algoritmo de matching negociable
- [x] **V**aluable — el usuario sabe si vale la pena postularse
- [x] **E**stimable — incertidumbre NLP, estimable como L
- [x] **S**mall — acotada al cálculo (regla <90% es HU-017)
- [x] **T**estable — AC verificables con pares CV/vacante conocidos
