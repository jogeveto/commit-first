---
id: HU-014
titulo: Exportar el CV en PDF y DOCX
epica: EP-005
prioridad: Must
complejidad: M
estado: lista
---

# Exportar el CV en PDF y DOCX

## Historia

Como **buscador de empleo**,
quiero **descargar el CV generado en PDF y en DOCX**,
para **postularme en los portales que exigen uno u otro formato**.

## Contexto

Complementa HU-013. El CV adaptado se exporta en ambos formatos. El PDF es además el insumo del auditor ATS (EP-006).

## Criterios de aceptación

### Escenario 1 — Happy path: descarga en ambos formatos
- **Dado que** tengo un CV generado para una vacante
- **Cuando** solicito exportar el CV en ambos formatos
- **Entonces** descargo un archivo PDF y un archivo DOCX, ambos con el contenido del CV generado y sin errores de renderizado

### Escenario 2 — Error: fallo al renderizar un formato
- **Dado que** solicito exportar el CV
- **Cuando** la generación de uno de los formatos falla
- **Entonces** recibo un mensaje de error para ese formato y puedo reintentar, sin descargar un archivo corrupto

### Escenario 3 — Edge case: contenido con caracteres especiales / tildes
- **Dado que** mi CV contiene tildes, ñ y caracteres especiales
- **Cuando** exporto a PDF y DOCX
- **Entonces** los caracteres se renderizan correctamente en ambos archivos

## Notas técnicas

- El PDF exportado debe ser texto seleccionable (no imagen) para que el auditor ATS (EP-006) lo lea.
- Considerar plantilla de CV consistente entre PDF y DOCX.

## Checklist INVEST

- [x] **I**ndependent — depende de HU-013; entrega la exportación
- [x] **N**egotiable — plantilla/estilo negociable
- [x] **V**aluable — el usuario descarga su CV usable
- [x] **E**stimable — librerías conocidas
- [x] **S**mall — acotada
- [x] **T**estable — AC verificables sobre los archivos generados
