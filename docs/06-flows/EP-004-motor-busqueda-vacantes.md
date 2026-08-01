---
id: flow-004-motor-busqueda-vacantes
epica: EP-004
historias_cubiertas: [HU-010, HU-011, HU-012, HU-020]
---

# Flow 004 — Motor de Búsqueda de Vacantes

## Resumen

Actor: **Buscador de empleo**. Objetivo: buscar vacantes bajo demanda en un portal, sin repetir las ya vistas, consultar el historial persistente y hacer seguimiento de sus postulaciones (qué envió y qué le falta). Condición de éxito: el usuario ve solo vacantes nuevas para él, su historial queda registrado y sabe en todo momento qué tiene "por postular" vs. "postulada".

## Diagrama

```mermaid
sequenceDiagram
  participant BE as Buscador de empleo
  participant Sistema
  participant Portal

  %% HU-010
  BE->>Sistema: Define criterio (cargo, ubicación) y ejecuta búsqueda
  Sistema->>Portal: Consulta vacantes (API o scraping)
  alt portal responde
    Portal-->>Sistema: Lista de vacantes
    %% HU-011
    Sistema->>Sistema: Filtra las ya vistas (Vacante_ID × User_ID)
    Sistema-->>BE: Muestra solo vacantes nuevas para mí
  else portal caído / error
    %% HU-010
    Sistema-->>BE: "No se pudo completar", reintentar (sin datos corruptos)
  else sin coincidencias
    %% HU-010
    Sistema-->>BE: Estado vacío "sin resultados"
  end

  %% HU-010
  Note over Sistema,Portal: Performance: resultados en < 30 s (no-funcional)

  %% HU-011
  Sistema->>Sistema: Marca vacantes presentadas como "vistas"
  alt fallo al registrar vista
    %% HU-011
    Sistema->>Sistema: Reporta y reintenta (no marca vista si no se guardó)
  end

  %% HU-012
  BE->>Sistema: Abre su historial de vacantes
  alt tiene historial
    Sistema-->>BE: Lista con estado (vista / cv_generado / postulada / descartada)
  else historial vacío
    %% HU-012
    Sistema-->>BE: Estado vacío informativo (nunca de otro usuario)
  else filtro inválido
    %% HU-012
    Sistema-->>BE: Reporta filtro inválido, mantiene historial visible
  end

  %% HU-020
  BE->>Sistema: Marca una vacante (con CV) como "postulada"
  alt persistencia OK
    %% HU-020
    Sistema-->>BE: Registra postulada + fecha; deja de contar como "por postular"
  else fallo al persistir
    %% HU-020
    Sistema-->>BE: Informa error; mantiene el estado anterior (sin ambigüedad)
  end
  %% HU-020
  Sistema-->>BE: Muestra conteo "por postular" (CV listo, no enviado) vs "postuladas"
```

## Trazabilidad

| Paso | HU | AC |
|---|---|---|
| Búsqueda devuelve vacantes | HU-010 | AC-1 (happy) |
| Portal no responde / falla | HU-010 | AC-2 (error) |
| Búsqueda sin resultados | HU-010 | AC-3 (edge) |
| Búsqueda responde < 30 s | HU-010 | AC-4 (performance) |
| No repite vacantes vistas | HU-011 | AC-1 (happy) |
| Fallo al registrar vista | HU-011 | AC-2 (error) |
| Misma vacante es nueva para otro usuario | HU-011 | AC-3 (edge) |
| Consulta historial | HU-012 | AC-1 (happy) |
| Filtro inválido | HU-012 | AC-2 (error) |
| Historial vacío para usuario nuevo | HU-012 | AC-3 (edge) |
| Marca vacante como postulada | HU-020 | AC-1 (happy) |
| Fallo al persistir postulación | HU-020 | AC-2 (error) |
| Postulada no se ofrece como pendiente | HU-020 | AC-3 (edge) |
| Conteo "por postular" vs "postuladas" | HU-020 | AC-4 (edge) |

## Notas

Dependencia externa (portal de terceros) mitigada con enfoque mock-first e interfaz de "proveedor de vacantes" (ver notas de HU-010). Las 4 HU quedan cubiertas (HU-010, HU-011, HU-012, HU-020).
