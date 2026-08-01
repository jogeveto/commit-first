---
name: trycore-escribir-criterios-aceptacion-bdd
description: Escribe los Acceptance Criteria de una Historia de Usuario en formato Given/When/Then (BDD/Gherkin), incluyendo happy path, error y edge case. Reemplaza la sección "Criterios de aceptación" de la historia. Path-scope principal: edita archivos `HU-*.md` bajo `docs/04-historias/`. Fase del pipeline: 4b de 7 (obligatoria, complementa a escribir-historia-usuario).
category: Discovery
tags: [acceptance-criteria, bdd, gherkin, given-when-then, trycore]
---

# trycore-escribir-criterios-aceptacion-bdd

Toma una historia de usuario existente (`docs/04-historias/HU-XXX-<slug>.md`) y completa su sección de AC con 3-5 escenarios en Given/When/Then.

## Contexto (fuente: METODOLOGIA.md §4)

Sintaxis:

```
Dado que [contexto inicial / estado del sistema]
Cuando [acción única realizada]
Entonces [resultado observable]
[Y opcionalmente más resultados]
```

Compatible con Cucumber, Behave, SpecFlow → los AC bien escritos se convierten en tests automatizados.

## Inputs requeridos

- Path a la historia objetivo, o ID `HU-XXX`. Si hay ambigüedad, listar las que no tienen AC aún y preguntar.

## Reglas duras

1. **3-5 escenarios** por historia. Si necesitas más, la historia es demasiado grande — sugerir división.
2. **Cobertura mínima obligatoria**:
   - 1 **Happy path** (flujo exitoso principal).
   - 1 **Error** (fallo esperado: validación, no autorizado, no encontrado, etc.).
   - 1 **Edge case** (caso límite que un QA experimentado señalaría).
3. **Given describe ESTADO, no acción**. "Dado que estoy en la página de búsqueda" ✓. "Dado que hago clic en buscar" ✗ (eso es un When).
4. **When describe UNA acción** del usuario o del tiempo. "Cuando escribo 3 caracteres y presiono Enter y luego..." → dividir en escenarios.
5. **Then describe resultado OBSERVABLE**: cambio visible en UI, dato persistido, evento emitido, error mostrado. Nada de "el sistema mejora la performance".
6. **Cada escenario es independiente** y testable de forma aislada.

## Flujo de la skill

1. **Cargar la historia** y validar que existe la sección "Criterios de aceptación" como placeholder.
2. **Razonar 3-5 escenarios** que cubran happy + error + edge.
3. **Para cada escenario**: escribir título descriptivo + bloque G/W/T.
4. **Reemplazar la sección AC** en el archivo (sin tocar el resto de la historia).
5. **Auto-check**: contar escenarios, verificar formato, marcar cualquier "When compuesto" o "Then no observable".
6. **Reportar** los escenarios escritos y cualquier ambigüedad detectada.

## Handoff

- **Agente revisor**: `bdd-validator`. Audita formato y cobertura.
- **Siguiente paso** (recomendado): correr `/trycore:invest` sobre la historia para validar INVEST ahora que tiene AC (la "T" de Testable se vuelve verificable).

## Ejemplo (de METODOLOGIA.md)

Historia: "Búsqueda inteligente de cursos con sugerencias"

```markdown
### Escenario 1 — Happy path: sugerencias en tiempo real
- **Dado que** estoy en la página principal
- **Cuando** escribo al menos 3 caracteres en el buscador
- **Entonces** aparecen sugerencias de cursos en tiempo real (máximo 500ms)

### Escenario 2 — Error: sin resultados
- **Dado que** busco un término que no coincide con ningún curso
- **Cuando** se muestran los resultados
- **Entonces** veo un mensaje "Sin resultados" con cursos sugeridos por categoría

### Escenario 3 — Edge case: navegación por teclado
- **Dado que** el listado de sugerencias está visible
- **Cuando** presiono flecha abajo y luego Enter
- **Entonces** navego al curso seleccionado sin necesidad de mouse
```

## Prompt útil (cuando el usuario te pasa la historia)

> "Dada la siguiente user story: `[paste]`. Genera los AC en G/W/T. Incluye un escenario del happy path, uno de error y un edge case que un QA experimentado consideraría."
