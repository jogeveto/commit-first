---
id: flow-003-ingesta-perfil-base
epica: EP-003
historias_cubiertas: [HU-007, HU-008, HU-009]
---

# Flow 003 — Ingesta y Perfil Base

## Resumen

Actor: **Buscador de empleo**. Objetivo: construir el perfil base automáticamente desde LinkedIn al entrar, complementarlo opcionalmente con un PDF (LLM local, v1.1) y consultarlo persistido. Condición de éxito: el usuario tiene un perfil estructurado sin captura manual.

## Diagrama

```mermaid
sequenceDiagram
  participant BE as Buscador de empleo
  participant Sistema
  participant LinkedIn
  participant LLM as LLM local

  %% HU-007
  BE->>Sistema: Inicia sesión (primera vez, sin perfil)
  Sistema->>LinkedIn: Solicita datos del perfil (scope OAuth2)
  alt LinkedIn responde datos
    %% HU-007
    LinkedIn-->>Sistema: Campos autorizados del perfil
    Sistema-->>BE: Crea perfil base con los campos disponibles
  else campos parciales
    %% HU-007
    LinkedIn-->>Sistema: Solo algunos campos
    Sistema-->>BE: Persiste disponibles, marca faltantes como pendientes
  else LinkedIn falla / no expone datos
    %% HU-007
    Sistema-->>BE: Perfil base vacío + aviso "complétalo manual o por PDF"
  end

  %% HU-008
  BE->>Sistema: (Opcional, v1.1) Sube PDF de hoja de vida
  Sistema->>LLM: Extrae info faltante en crudo
  alt PDF válido con datos
    %% HU-008
    LLM-->>Sistema: Campos extraídos
    Sistema-->>BE: Completa campos faltantes y persiste
  else PDF inválido/corrupto
    %% HU-008
    Sistema-->>BE: Rechaza archivo, no altera perfil
  else PDF válido sin datos aprovechables
    %% HU-008
    Sistema-->>BE: "No encontré información", perfil sin cambios
  else conflicto con dato existente
    %% HU-008
    Sistema-->>BE: Notifica conflicto con ambos valores, pide elegir
  end

  %% HU-009
  BE->>Sistema: Vuelve y abre su perfil
  alt perfil existe
    Sistema-->>BE: Muestra perfil estructurado persistido (solo el suyo)
  else perfil aún no creado
    %% HU-009
    Sistema-->>BE: Indica que no hay perfil todavía (sin error)
  end
```

## Trazabilidad

| Paso | HU | AC |
|---|---|---|
| Perfil autopoblado desde LinkedIn | HU-007 | AC-1 (happy) |
| LinkedIn no devuelve datos → perfil vacío + aviso | HU-007 | AC-2 (error) |
| Campos parciales → persiste disponibles | HU-007 | AC-3 (edge) |
| PDF completa campos faltantes | HU-008 | AC-1 (happy) |
| PDF inválido/corrupto rechazado | HU-008 | AC-2 (error) |
| PDF contradice dato existente → conflicto | HU-008 | AC-3 (edge) |
| PDF sin info aprovechable | HU-008 | AC-4 (edge) |
| Recupera perfil persistido | HU-009 | AC-1 (happy) |
| Consulta de perfil inexistente | HU-009 | AC-2 (error) |
| Aislamiento del perfil entre usuarios | HU-009 | AC-3 (edge) |

## Notas

HU-008 está diferida a v1.1 (marcada en el flow como opcional); se incluye para completar la trazabilidad de la épica. Las 3 HU quedan cubiertas.
