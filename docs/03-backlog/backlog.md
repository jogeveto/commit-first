# Backlog — Asistente de Empleabilidad IA

> El orden de las filas **es** la priorización. Para detalles de priorización ver `docs/05-priorizacion/`.

**Última actualización**: 2026-07-31
**Framework activo**: Valor-Esfuerzo

## Resumen

- Total historias: 19
- En estado `lista`: 19
- En estado `en-curso`: 0
- En estado `hecha`: 0

> Nota: el orden de filas refleja la priorización **Valor/Esfuerzo** vigente (`docs/05-priorizacion/valor-esfuerzo-2026-07-31.md`): orden de ejecución que combina valor de cuadrante con dependencias de cimiento. HU-008 (skip/aplazar) queda al final como v1.1.

## Tabla priorizada

> La columna **Scope** indica pertenencia al alcance (In = dentro del alcance construible; v1.1 = diferido). El framework activo es **Valor/Esfuerzo**, no MoSCoW: el valor relativo lo expresa la columna **Cuadrante**, no una etiqueta Must/Should. Alineado con "Producto completo, no MVP": el alcance In se construye entero.

| # | ID | Título | Épica | Scope | Complejidad | Cuadrante | Estado | AC | Notas |
|---|----|--------|-------|-------|-------------|-----------|--------|----|-------|
| 1 | HU-001 | Iniciar sesión con LinkedIn (OAuth2) | EP-001 | In | M | Quick win | lista | 3 | Cimiento — puerta de entrada |
| 2 | HU-002 | Provisión automática de cuenta en primer acceso | EP-001 | In | S | Quick win | lista | 3 | Cimiento |
| 3 | HU-003 | Emisión y validación de sesión ligada al User_ID | EP-001 | In | M | Quick win | lista | 3 | Cimiento — habilita tenancy |
| 4 | HU-004 | Esquema de datos multi-tenant con User_ID | EP-002 | In | M | Quick win | lista | 3 | Cimiento transversal |
| 5 | HU-005 | Capa de acceso a datos que filtra por tenant | EP-002 | In | M | Quick win | lista | 3 | Cimiento transversal |
| 6 | HU-006 | Guarda de autorización y prueba de aislamiento | EP-002 | In | M | Quick win | lista | 3 | Cimiento — KPI O3 (fugas=0) |
| 7 | HU-007 | Extraer perfil base desde LinkedIn | EP-003 | In | M | Quick win | lista | 3 | Spike scope LinkedIn |
| 8 | HU-009 | Persistir y consultar el perfil estructurado | EP-003 | In | S | Quick win | lista | 3 | |
| 9 | HU-010 | Buscar vacantes bajo demanda en un portal | EP-004 | In | L | Big bet | lista | 4 | Decisión API/scraping en sprint; mock-first |
| 10 | HU-011 | Deduplicar vacantes ya vistas por el usuario | EP-004 | In | M | Quick win | lista | 3 | Dedup por User_ID |
| 11 | HU-012 | Persistir y consultar el historial de vacantes | EP-004 | In | S | Fill-in | lista | 3 | |
| 12 | HU-013 | Generar un CV adaptado a las palabras clave | EP-005 | In | L | Big bet | lista | 4 | Spike latencia LLM; fijar N en sprint |
| 13 | HU-014 | Exportar el CV en PDF y DOCX | EP-005 | In | M | Quick win | lista | 3 | PDF con texto seleccionable (lo consume el auditor HU-016; HU-014 no depende de HU-016) |
| 14 | HU-015 | Gestionar los prompts del LLM en base de datos | EP-005 | In | M | Fill-in | lista | 3 | Sin panel admin (Non-goal) |
| 15 | HU-016 | Calcular el % de match ATS | EP-006 | In | L | Big bet | lista | 4 | Microservicio Python; mock-first TF-IDF; consume el PDF de HU-014 |
| 16 | HU-017 | Formulario de enriquecimiento cuando match < 90% | EP-006 | In | M | Quick win | lista | 3 | Regla dura 90% (O2) |
| 17 | HU-018 | Sugerir descarte y persistir la decisión | EP-006 | In | S | Fill-in | lista | 3 | Cierra flujo end-to-end |
| 18 | HU-020 | Marcar y hacer seguimiento de la postulación | EP-004 | In | S | Quick win | lista | 4 | Registro manual (no auto-aplicar); cierra vacío de seguimiento |
| 19 | HU-008 | Complementar el perfil subiendo un PDF (LLM local) | EP-003 | v1.1 | L | Skip → v1.1 | lista | 4 | Diferida a v1.1; spike LLM + slice a/b |

## Trazabilidad rápida (épica → historias)

- **EP-001 — Autenticación e Identidad (LinkedIn OAuth2)**: HU-001, HU-002, HU-003
- **EP-002 — Aislamiento Multi-tenant y Modelo de Datos**: HU-004, HU-005, HU-006
- **EP-003 — Ingesta y Perfil Base**: HU-007, HU-008, HU-009
- **EP-004 — Motor de Búsqueda de Vacantes**: HU-010, HU-011, HU-012, HU-020
- **EP-005 — Generación de CV y Gestor de Prompts**: HU-013, HU-014, HU-015
- **EP-006 — Auditoría ATS y Enriquecimiento**: HU-016, HU-017, HU-018

## Items pendientes de definir

- [ ] HU-019 — Segundo portal de búsqueda (reservado, v1.1; se detalla más adelante).

## Consistencias verificadas

- 19/19 historias con épica asignada (0 huérfanas).
- 19/19 con AC (0 con count=0 en estado `lista`).
- 6/6 épicas con ≥1 historia.
- Distribución de complejidad: S=6, M=8, L=5.
- Distribución de scope: In=18, v1.1=1 (HU-008).
- Distribución de cuadrante: Quick win=12, Big bet=3, Fill-in=3, Skip/v1.1=1.
