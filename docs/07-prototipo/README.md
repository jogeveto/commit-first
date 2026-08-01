# Prototipo clickeable — DESIGN_SOURCE

Fuente de verdad del **gate de fidelidad visual** del build harness. Todo slice con UI se verifica contra este prototipo (`ux-fidelity-reviewer`).

## Abrir

Abre `index.html` en cualquier navegador. Es autocontenido (solo requiere internet para las fuentes de Google Fonts). Punto de entrada: pantalla de login → botón "Continuar con LinkedIn".

## Cobertura — 100% del journey (sitemap + flows)

| Pantalla | Épica | Historias representadas |
|---|---|---|
| Login LinkedIn | EP-001 | HU-001, HU-002 |
| Mi perfil (LinkedIn + PDF) | EP-003 | HU-007, HU-008, HU-009 |
| Buscar vacantes (+ dedup) | EP-004 | HU-010, HU-011 |
| Generar CV (+ export, prompts) | EP-005 | HU-013, HU-014, HU-015 |
| Auditoría ATS (gauge + keywords) | EP-006 | HU-016 |
| Enriquecimiento < 90% | EP-006 | HU-017 |
| Descarte de vacante | EP-006 | HU-018 |
| Historial (estados, reactivar) | EP-004 | HU-012, HU-018 |

Elementos transversales: selector de usuario en el header (aislamiento **multi-tenant** EP-002 visible), pill de `tenant`, nota de IA local (privacidad, O4).

## Interacciones clave (clickeables)

- **Login** → entra a la app.
- **Rail izquierdo** → navega el recorrido (stepper con progreso).
- **Vacante** → genera CV.
- **Auditoría**: "Enriquecer mi CV" → modal → regenera y el medidor sube de **78% a 92%** (cruza el umbral, verde). "Descartar" → modal → va al historial.
- **Selector de usuario** (header) → alterna Ana/Diego mostrando datos aislados.
- **Historial** → filtros por estado (vista / con CV / descartada).

## Estética (para el gate de fidelidad)

- Fondo papel marfil, titulares serif **Fraunces**, UI **Hanken Grotesk**, datos **JetBrains Mono**.
- Acento verde pino; señalética de match **rojo→ámbar→verde**.
- Medidor de match como héroe; chips de keywords (presentes vs faltantes).

## Estado

`design_source.confirmed=false` en `build-state.json` hasta **revisión y aprobación humana**. Al aprobarlo, pasa a `true` y habilita las historias con UI contra esta referencia.
