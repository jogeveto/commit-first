# User Story Map — Asistente de Empleabilidad IA

> Estilo Jeff Patton. Eje X = actividades del usuario en orden cronológico. Eje Y = prioridad (arriba = más prioritario). Líneas horizontales = releases.

## Backbone (actividades del usuario)

```
1. Entrar  →  2. Preparar mi perfil  →  3. Buscar vacantes  →  4. Generar mi CV  →  5. Auditar y decidir
```

Cada actividad del backbone mapea a las épicas: (1)→EP-001, cimiento transversal EP-002; (2)→EP-003; (3)→EP-004; (4)→EP-005; (5)→EP-006.

## Mapa

| Actividad → | 1. Entrar | 2. Preparar perfil | 3. Buscar vacantes | 4. Generar CV | 5. Auditar y decidir |
|---|---|---|---|---|---|
| **Release 1 — MVP** | HU-001 (login LinkedIn) | HU-007 (perfil desde LinkedIn) | HU-010 (buscar en un portal) | HU-013 (CV adaptado) | HU-016 (% match ATS) |
| | HU-002 (provisión cuenta) | HU-009 (persistir perfil) | HU-011 (dedup vacantes) | HU-014 (export PDF/DOCX) | HU-017 (form enriquecimiento <90%) |
| | HU-003 (sesión por User_ID) | | HU-012 (historial vacantes) | HU-015 (prompts en BD) | HU-018 (sugerir descarte + persistir) |
| | HU-004 (esquema multi-tenant) *(cimiento transversal)* | | HU-020 (seguimiento de postulación) | | |
| | HU-005 (acceso filtrado por tenant) *(cimiento transversal)* | | | | |
| | HU-006 (guarda + prueba aislamiento) *(cimiento transversal)* | | | | |
| `─── corte MVP ───` | | | | | |
| **Release 2 — v1.1** | | HU-008 (complementar con PDF vía LLM) | HU-019 (2º portal de búsqueda, ID reservado) | | |

## Narrativa del journey

1. **Entrar**: el usuario llega y se autentica con LinkedIn (sin contraseñas). En primer acceso se provisiona su cuenta y toda su sesión queda ligada a su `User_ID`. Transversalmente, el cimiento multi-tenant (esquema, filtrado por tenant, guarda de aislamiento) garantiza que sus datos nunca se crucen con los de otro usuario. Es la base sobre la que todo lo demás persiste.
2. **Preparar mi perfil**: al entrar, el sistema autopobla su perfil base desde LinkedIn y lo persiste. Opcionalmente (v1.1) puede subir un PDF para que el LLM local complete la información faltante sin formularios largos.
3. **Buscar vacantes**: bajo demanda, el usuario busca vacantes en un portal. El sistema nunca le muestra una vacante que ya vio (dedup por `Vacante_ID × User_ID`) y guarda el historial de forma permanente.
4. **Generar mi CV**: elige una vacante y el LLM local genera un CV adaptado a sus palabras clave, exportable en PDF y DOCX. Los prompts que guían al LLM viven en PostgreSQL y se editan en caliente.
5. **Auditar y decidir**: el auditor ATS en Python calcula el % de match del CV contra la vacante. Si es < 90%, el sistema ofrece un formulario para enriquecer la info o sugiere descartar la vacante; toda decisión se persiste. Aquí se cierra el flujo end-to-end.

## Justificación del MVP

La línea de corte MVP incluye **HU-001 a HU-007, HU-009 a HU-018** (todo menos HU-008) porque cubren el journey **mínimo viable completo**: el usuario puede entrar, tener perfil, buscar, generar CV y auditarlo con la regla de enriquecimiento < 90%. Esto entrega valor real end-to-end.

- **Cimiento no negociable**: EP-001 (identidad) y EP-002 (aislamiento multi-tenant) están enteros en el MVP porque sin identidad ni tenancy ninguna otra actividad funciona ni es segura. Por eso la columna "Entrar" tiene 6 historias (más peso que las demás): concentra ambas épicas de cimiento.
- **Cierre del ciclo de valor**: sin la actividad 5 (auditar y decidir) el producto no cumple su promesa (superar el ATS), así que HU-016/017/018 entran al MVP.
- **Lo diferible**: solo **HU-008** (complementar el perfil subiendo un PDF vía LLM) queda en v1.1. Razón: el perfil ya se autopobla desde LinkedIn (HU-007), que es suficiente para completar el journey; el PDF es un enriquecimiento de la ingesta, no un bloqueante. Un segundo portal de búsqueda también es post-MVP (un portal basta para validar el flujo).

## Gaps detectados

- **Dependencia de secuencia dura**: la actividad 4 (Generar CV) requiere que existan perfil (act. 2) y vacante (act. 3). El orden de construcción debe respetar EP-003 y EP-004 antes de EP-005. Documentado en el Anexo A del PRD.
- **Riesgo externo en actividad 3**: la búsqueda depende de scraping/APIs de portales de terceros (LinkedIn/Computrabajo/El Empleo), fuera de nuestro control. Mitigación: empezar el MVP con un solo portal (el más estable/accesible) y aislar el motor tras una interfaz para agregar portales sin tocar el resto.
- **HU-015 (gestor de prompts)** es infraestructura de la actividad 4 más que journey visible del usuario; se mantiene en el MVP porque la calidad del CV generado depende de poder ajustar prompts sin re-desplegar.
