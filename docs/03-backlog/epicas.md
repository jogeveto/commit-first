# Épicas — Asistente de Empleabilidad IA

> Cada épica se traza a ≥ 1 objetivo del PRD (sección 1).
> Cada historia futura se traza a ≥ 1 épica.
> Orden de construcción: cimiento (EP-001, EP-002) antes que negocio (EP-003…EP-006).

## Mapa de trazabilidad

| Épica | Objetivos del PRD que cubre | Capability | Fase (Anexo A) | Tipo |
|---|---|---|---|---|
| EP-001 — Autenticación e Identidad (LinkedIn OAuth2) | O5 | Cap 1 | Fase 1 | Cimiento |
| EP-002 — Aislamiento Multi-tenant y Modelo de Datos | O3 | Cap 6 | Fase 2 | Cimiento |
| EP-003 — Ingesta y Perfil Base | O5 | Cap 2 | Fase 3 | Negocio |
| EP-004 — Motor de Búsqueda de Vacantes | O1 (parcial) | Cap 3 | Fase 4 | Negocio |
| EP-005 — Generación de CV y Gestor de Prompts | O1, O2, O4 | Cap 4 | Fase 5 | Negocio |
| EP-006 — Auditoría ATS y Enriquecimiento | O1, O2 | Cap 5 | Fase 6 | Negocio |

**Cobertura de objetivos** — O1: EP-004 (parcial) + EP-005 + EP-006 (el KPI "vacante→PDF < 5 min" exige las 3 encadenadas; ninguna sola lo cumple) · O2: EP-005/006 (EP-005 genera el CV, precondición del match ≥90%) · O3: EP-002 · O4: EP-005 (LLM local, sin IA de pago) · O5: EP-001/003. Todos los objetivos cubiertos; toda épica cubre ≥1 objetivo; sin solapes de capability.

---

## EP-001 — Autenticación e Identidad (LinkedIn OAuth2)

**layer**: foundational
**OpenSpec Change**: autenticacion-identidad-linkedin-oauth2 (en construcción)

**Resumen**: Provee el único mecanismo de entrada al sistema: login vía LinkedIn OAuth2, sin contraseñas ni registro manual. En el primer login se provisiona la cuenta y se emite una sesión/JWT ligada al `User_ID`, que será la clave de aislamiento de todo el sistema.

**Justificación**: Cubre O5 (100% de altas sin formulario manual). Es cimiento: ninguna otra épica funciona sin identidad de usuario. (Nota: O4 —costo IA $0— se materializa en EP-005 con el LLM local, no aquí; EP-001 no tiene AC que verifique costo.)

**Capabilities incluidas**:
- Cap 1.1 — Login exclusivo vía LinkedIn OAuth2.
- Cap 1.2 — Emisión de sesión/JWT ligada al `User_ID`.
- Cap 1.3 — Provisión automática de cuenta en primer login.
- Cap 1.4 — Cierre de sesión (contraparte de 1.2: el equipo se comparte entre personas).

**Historias previstas**:
- HU-001 — Iniciar sesión con LinkedIn (OAuth2).
- HU-002 — Provisión automática de cuenta en primer acceso.
- HU-003 — Emisión y validación de sesión ligada al User_ID.
- HU-021 — Cerrar sesión. *(Añadida durante la construcción: la auditoría de coherencia detectó que el prototipo aprobado incluye el botón "Salir" sin ninguna HU que lo respaldara.)*

**Métrica de éxito de la épica**: 100% de los accesos ocurren vía LinkedIn OAuth2; 0 contraseñas almacenadas; toda petición autenticada porta un `User_ID` verificable (KPI O5).

---

## EP-002 — Aislamiento Multi-tenant y Modelo de Datos

**layer**: foundational

**Resumen**: Define el esquema PostgreSQL con todas las entidades del dominio (perfiles, vacantes, CVs, matches, prompts) ligadas a `User_ID`, y una capa de acceso a datos que filtra por tenant en cada consulta, con guardas de autorización. Garantiza que ningún usuario pueda leer datos de otro.

**Justificación**: Cubre O3 (aislamiento de datos 100%). Es cimiento transversal: todas las épicas de negocio persisten a través de esta capa.

**Capabilities incluidas**:
- Cap 6.1 — Todo registro pertenece a un `User_ID`; toda consulta filtra por él.
- Cap 6.2 — Pruebas de autorización que garantizan 0 fuga entre cuentas.

**Historias previstas**:
- HU-004 — Esquema de datos multi-tenant con User_ID en toda entidad.
- HU-005 — Capa de acceso a datos que filtra por tenant.
- HU-006 — Guarda de autorización y prueba de aislamiento entre usuarios.

**Métrica de éxito de la épica**: 0 registros accesibles fuera del `User_ID` propietario, verificado por prueba de autorización automatizada (KPI O3: fugas = 0).

---

## EP-003 — Ingesta y Perfil Base

**layer**: business

**Resumen**: Al iniciar sesión, el sistema extrae automáticamente la información del perfil de LinkedIn para construir la base. Opcionalmente el usuario sube un PDF y el LLM local extrae la información faltante en crudo, evitando formularios manuales largos. El perfil queda persistido por usuario.

**Justificación**: Cubre O5 (base del perfil sin captura manual). Depende de EP-001 (identidad) y EP-002 (persistencia).

**Capabilities incluidas**:
- Cap 2.1 — Extracción automática del perfil desde LinkedIn.
- Cap 2.2 — Complemento opcional vía PDF con extracción por LLM local.
- Cap 2.3 — Almacenamiento del perfil estructurado por `User_ID`.

**Historias previstas**:
- HU-007 — Extraer perfil base desde LinkedIn al iniciar sesión.
- HU-008 — Complementar el perfil subiendo un PDF (extracción por LLM local).
- HU-009 — Persistir y consultar el perfil estructurado del usuario.

**Métrica de éxito de la épica**: el perfil base se autopobla sin formularios manuales, extrayendo al menos los campos mínimos del perfil LinkedIn (nombre, experiencia, educación, habilidades) verificable por inspección del registro persistido; el usuario puede complementar con PDF; 100% del perfil queda persistido por usuario (contribuye a O5).

---

## EP-004 — Motor de Búsqueda de Vacantes

**layer**: business

**Resumen**: Busca vacantes bajo demanda por usuario mediante scraping o consumo de APIs de portales (LinkedIn, Computrabajo, El Empleo). Deduplica guardando `Vacante_ID × User_ID` para no repetir ofertas a la misma persona, y persiste el historial de forma permanente.

**Justificación**: Cubre O1 (reducir tiempo de búsqueda). Depende de EP-002 (persistencia y tenancy).

**Capabilities incluidas**:
- Cap 3.1 — Búsqueda bajo demanda en ≥1 portal.
- Cap 3.2 — Deduplicación por `Vacante_ID × User_ID`.
- Cap 3.3 — Persistencia permanente del historial de vacantes.
- Cap 3.4 — Seguimiento de postulación: marcar vacantes ya enviadas y ver pendientes por enviar (registro manual, sin auto-aplicar).

**Historias previstas**:
- HU-010 — Buscar vacantes bajo demanda en un portal.
- HU-011 — Deduplicar vacantes ya vistas por el usuario.
- HU-012 — Persistir y consultar el historial de vacantes.
- HU-020 — Marcar y hacer seguimiento de la postulación.

**Métrica de éxito de la épica**: la búsqueda responde en < 30 s y nunca muestra al usuario una vacante ya vista; historial persistente; el usuario ve en todo momento cuántas vacantes tiene "por postular" vs. "postuladas" (KPI O1 parcial).

---

## EP-005 — Generación de CV y Gestor de Prompts

**layer**: business

**Resumen**: Dado un perfil y una vacante, el LLM local genera un CV adaptado a las palabras clave de la vacante, exportable en PDF y DOCX. Los prompts que usa el LLM están parametrizados en PostgreSQL y son editables en caliente sin re-desplegar.

**Justificación**: Cubre O1 (tiempo < 5 min) y O4 (costo $0 con LLM local). Depende de EP-003 (perfil) y EP-004 (vacante).

**Capabilities incluidas**:
- Cap 4.1 — Generación de CV adaptado (LLM local).
- Cap 4.2 — Exportación a PDF y DOCX.
- Cap 4.3 — Gestor dinámico de prompts en PostgreSQL, editable en caliente.

**Historias previstas**:
- HU-013 — Generar un CV adaptado a las palabras clave de la vacante.
- HU-014 — Exportar el CV en PDF y DOCX.
- HU-015 — Gestionar los prompts del LLM en base de datos (edición en caliente).

**Métrica de éxito de la épica**: se genera y descarga un CV adaptado en PDF/DOCX; los prompts se editan en BD y el cambio surte efecto sin re-desplegar; 0 costo de inferencia (KPI O1, O4).

---

## EP-006 — Auditoría ATS y Enriquecimiento

**layer**: business

**Resumen**: Un microservicio/scripts en Python con NLP escanea el CV generado y lo compara con la vacante real, entregando un % de match exacto. Si el match es < 90%, el sistema genera un formulario en React para pedir la información faltante o sugiere descartar la vacante, persistiendo permanentemente toda decisión. Cierra el flujo end-to-end.

**Justificación**: Cubre O1 (cierre del flujo en < 5 min) y O2 (≥80% de CVs con match ≥90% tras enriquecimiento). Depende de EP-005 (CV generado).

**Capabilities incluidas**:
- Cap 5.1 — Auditor ATS en Python (NLP) que entrega % de match exacto.
- Cap 5.2 — Regla < 90%: formulario de enriquecimiento o sugerencia de descarte.
- Cap 5.3 — Persistencia permanente de decisiones (CV, match, descarte, enriquecimiento).

**Historias previstas**:
- HU-016 — Calcular el % de match ATS entre el CV y la vacante.
- HU-017 — Ofrecer formulario de enriquecimiento cuando el match < 90%.
- HU-018 — Sugerir descarte y persistir la decisión de la vacante.

**Métrica de éxito de la épica**: el auditor entrega un % de match numérico; la regla < 90% dispara enriquecimiento o descarte; toda decisión persiste (KPI O2: ≥80% de CVs alcanzan ≥90% tras ≤1 enriquecimiento).
