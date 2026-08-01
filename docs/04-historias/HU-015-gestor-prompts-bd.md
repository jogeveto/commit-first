---
id: HU-015
titulo: Gestionar los prompts del LLM en base de datos (edición en caliente)
epica: EP-005
prioridad: Must
complejidad: M
estado: lista
---

# Gestionar los prompts del LLM en base de datos (edición en caliente)

## Historia

Como **administrador técnico del sistema**,
quiero **modificar los prompts que usa el LLM y que el cambio aplique sin re-desplegar**,
para **que el buscador reciba CVs de mejor calidad de forma inmediata tras un ajuste**.

## Contexto

Infraestructura de EP-005. Los prompts (generación de CV, extracción de PDF) viven parametrizados en PostgreSQL y son editables en caliente. Nota: la gestión admin se hace vía BD (Non-goal: no hay panel admin). Verificar que no duplica AC del capability de generación (HU-013).

## Criterios de aceptación

### Escenario 1 — Happy path: un prompt editado aplica sin re-desplegar
- **Dado que** un prompt está almacenado en la base de datos y en uso
- **Cuando** el administrador técnico modifica su texto (por el canal que el equipo defina: SQL directo, endpoint interno o CLI)
- **Entonces** la siguiente generación usa el prompt actualizado sin necesidad de reiniciar ni re-desplegar la aplicación

### Escenario 2 — Error: falta el prompt requerido
- **Dado que** el prompt que una operación necesita no existe en la base de datos
- **Cuando** se dispara esa operación (ej. generar CV)
- **Entonces** el sistema reporta el prompt faltante de forma controlada en lugar de ejecutar con un prompt vacío

### Escenario 3 — Edge case: parámetros del prompt (placeholders) sin resolver
- **Dado que** un prompt contiene placeholders (ej. datos del perfil o de la vacante)
- **Cuando** se usa para una generación
- **Entonces** todos los placeholders se sustituyen por valores reales; si alguno no se puede resolver, la operación se detiene con un error claro y no envía el placeholder crudo al LLM

## Notas técnicas

- Tabla de prompts versionada (id, clave, texto, versión, activo).
- Sin caché agresiva que impida el "en caliente" (o invalidación por versión).

## Checklist INVEST

- [x] **I**ndependent — soporta a HU-013 pero se entrega/prueba por separado
- [x] **N**egotiable — modelo de versionado negociable
- [x] **V**aluable — habilita ajustar la calidad sin desplegar
- [x] **E**stimable — acotada
- [x] **S**mall — media
- [x] **T**estable — AC verificables
