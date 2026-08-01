---
id: flow-005-generacion-cv-prompts
epica: EP-005
historias_cubiertas: [HU-013, HU-014, HU-015]
---

# Flow 005 — Generación de CV y Gestor de Prompts

## Resumen

Actores: **Buscador de empleo** (genera y exporta el CV) y **Administrador técnico** (gestiona los prompts en BD). Objetivo: producir un CV adaptado a la vacante con el LLM local, exportarlo en PDF/DOCX, usando prompts editables en caliente. Condición de éxito: el usuario descarga un CV adaptado sin costo de inferencia.

## Diagrama

```mermaid
sequenceDiagram
  participant AT as Administrador técnico
  participant BE as Buscador de empleo
  participant Sistema
  participant BD as PostgreSQL
  participant LLM as LLM local

  %% HU-015
  AT->>BD: Modifica el prompt (SQL / endpoint interno / CLI)
  Note over Sistema,BD: La siguiente generación usa el prompt actualizado sin re-desplegar

  %% HU-013
  BE->>Sistema: Selecciona vacante y solicita generar CV
  Sistema->>BD: Obtiene prompt activo
  alt falta el prompt requerido
    %% HU-015
    Sistema-->>BE: Reporta prompt faltante (no ejecuta con prompt vacío)
  else placeholder sin resolver
    %% HU-015
    Sistema-->>BE: Detiene con error (no envía placeholder crudo al LLM)
  else prompt OK
    Sistema->>LLM: Genera CV (perfil + vacante + prompt)
    alt perfil incompleto
      %% HU-013
      Sistema-->>BE: Indica qué falta en el perfil (no genera CV inválido)
    else LLM falla / excede tiempo
      %% HU-013
      Sistema-->>BE: Error controlado, reintentar (sin CV a medias)
    else generación OK
      %% HU-013
      LLM-->>Sistema: CV con ≥ umbral de palabras clave de la vacante
      Sistema-->>BE: CV disponible para revisar (dentro del presupuesto de tiempo)
    end
  end

  %% HU-014
  BE->>Sistema: Solicita exportar el CV en ambos formatos
  alt render OK
    Sistema-->>BE: Descarga PDF + DOCX (contenido del CV, sin errores de render)
  else fallo al renderizar un formato
    %% HU-014
    Sistema-->>BE: Error para ese formato, reintentar (sin archivo corrupto)
  end
  %% HU-014
  Note over Sistema,BE: Caracteres especiales/tildes se renderizan correctos en ambos
```

## Trazabilidad

| Paso | HU | AC |
|---|---|---|
| CV generado con umbral de palabras clave | HU-013 | AC-1 (happy) |
| Falta info base del perfil | HU-013 | AC-2 (error) |
| LLM falla/excede tiempo | HU-013 | AC-3 (edge) |
| Generación dentro del presupuesto de tiempo | HU-013 | AC-4 (performance) |
| Descarga en PDF y DOCX | HU-014 | AC-1 (happy) |
| Fallo al renderizar un formato | HU-014 | AC-2 (error) |
| Caracteres especiales/tildes | HU-014 | AC-3 (edge) |
| Prompt editado aplica sin re-desplegar | HU-015 | AC-1 (happy) |
| Falta el prompt requerido | HU-015 | AC-2 (error) |
| Placeholder sin resolver | HU-015 | AC-3 (edge) |

## Notas

Se usan dos actores válidos del PRD §Stakeholders: Buscador de empleo (usuario primario) y Administrador técnico (operaciones/admin, gestión vía BD sin panel admin). Las 3 HU quedan cubiertas.
