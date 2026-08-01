---
name: "TRYCORE: Mapa"
description: Crea un User Story Map estilo Jeff Patton (backbone cronológico + ranking + líneas de release) en docs/02-user-story-map/.
category: Discovery
tags: [story-map, jeff-patton, mvp, trycore]
---

Invoca el skill **trycore-crear-mapa-historias**.

---

## Preflight

- Verificar PRD y `epicas.md` existentes.

## Comportamiento

1. Leer PRD (sección 3, sección 5) y `epicas.md`.
2. Construir backbone (3-7 actividades cronológicas del journey).
3. Asignar historias existentes a columnas; reservar IDs para historias anticipadas.
4. Definir línea de MVP con justificación.
5. Detectar gaps (columnas con poca cobertura).
6. Escribir `docs/02-user-story-map/<slug>.md`.

## Próximo paso

`/trycore:historia HU-XXX` (loop) o pasar al `/trycore:flujo` para automatizar el loop.
