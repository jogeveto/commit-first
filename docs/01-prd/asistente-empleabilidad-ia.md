# PRD — Asistente de Empleabilidad IA

> **Estado**: draft
> **Versión**: 0.1
> **Última actualización**: 2026-07-31
> **Sponsor**: Propietario del producto (uso familiar cerrado)
> **Modo**: para-agentes (incluye Anexo A de fases secuenciales)

---

## 1. Introducción y objetivos

**Problema**: Superar los filtros automáticos (ATS — Applicant Tracking Systems) de los portales de empleo exige adaptar la hoja de vida a cada vacante. Hacerlo manualmente es lento, tedioso y no garantiza que el CV pase el filtro por palabras clave. El resultado es tiempo perdido y postulaciones descartadas antes de que un humano las lea.

**Contexto**: Los portales de empleo (LinkedIn, Computrabajo, El Empleo) usan ATS que rankean CVs por coincidencia de palabras clave con la descripción de la vacante. Un CV genérico obtiene bajo match. Adaptar cada CV a mano no escala. Existen herramientas SaaS de pago que hacen esto, pero envían datos personales a terceros y cobran suscripción. Este producto resuelve el mismo problema con **IA local** (privacidad total, costo cero de inferencia) para un grupo cerrado de usuarios (familia).

**Objetivos** (medibles):
- **O1** — Reducir el tiempo de adaptación de un CV a una vacante de ~30–45 min (manual) a **< 5 min** (asistido, desde vacante hasta PDF/DOCX descargado).
- **O2** — Lograr que ≥ **80%** de los CVs generados alcancen un **Match ATS ≥ 90%** contra la vacante objetivo tras, como máximo, una ronda de enriquecimiento.
- **O3** — Garantizar **aislamiento de datos 100%** entre usuarios: 0 registros (perfiles, vacantes, historial) accesibles fuera del `User_ID` propietario (verificable por pruebas de autorización).
- **O4** — Operar con **costo de inferencia IA = $0** usando exclusivamente un LLM local (sin llamadas a APIs de IA de pago).
- **O5** — Eliminar el registro manual: **100%** del alta de usuario y de la base del perfil se hace vía login LinkedIn (OAuth2), sin contraseñas ni formularios de registro.

## 2. Stakeholders

| Rol | Nombre / equipo | Interés / responsabilidad |
|---|---|---|
| Sponsor | Propietario del producto | Financia (tiempo), define alcance, decide releases |
| Product Owner | Propietario del producto | Prioriza backlog, acepta artefactos |
| Equipo técnico | Propietario del producto (dev) | Construye y mantiene el sistema |
| Usuario primario | Propietario + esposa | Usan la app para postularse a vacantes |
| Usuario secundario | Futuros miembros de familia | Cuentas adicionales aisladas (multi-tenant) |
| Operaciones / soporte / admin | Propietario (vía PostgreSQL) | **No hay panel admin**: gestión, limpieza y ajustes globales directo en BD |

## 3. Historias de Usuarios (de alto nivel)

> Detalle por historia en `docs/04-historias/`. Aquí solo el resumen narrativo.

- Como **buscador de empleo**, necesito **iniciar sesión con mi cuenta de LinkedIn** para **entrar sin crear contraseñas ni llenar formularios de registro**.
- Como **buscador de empleo**, necesito que el sistema **construya mi perfil base automáticamente desde LinkedIn (y opcionalmente desde un PDF)** para **no tener que capturar todo a mano**.
- Como **buscador de empleo**, necesito **buscar vacantes bajo demanda en varios portales** para **encontrar oportunidades sin repetir las que ya vi**.
- Como **buscador de empleo**, necesito **generar un CV adaptado a las palabras clave de una vacante** para **maximizar mi match en el ATS**.
- Como **buscador de empleo**, necesito **auditar mi CV contra la vacante y ver un % de match exacto** para **saber si vale la pena postularme**.
- Como **buscador de empleo**, necesito que **cuando el match sea < 90% el sistema me pida la información faltante o me sugiera descartar** para **mejorar el CV o no perder tiempo**.
- Como **usuario dueño de mis datos**, necesito que **mi información esté aislada de otros usuarios** para **garantizar mi privacidad**.
- Como **administrador técnico**, necesito **editar en caliente los prompts del LLM** para **ajustar la generación sin desplegar código**.

## 4. Componentes principales y sitemap

Bloques del sistema:

```
                        ┌────────────────────────────┐
   [LinkedIn OAuth2] ──▶│  Frontend React (SPA)      │
                        │  - Login LinkedIn          │
                        │  - Perfil / Ingesta        │
                        │  - Búsqueda de vacantes    │
                        │  - Generación de CV        │
                        │  - Auditoría ATS / Match   │
                        │  - Form de enriquecimiento │
                        └──────────────┬─────────────┘
                                       │ REST/JSON (JWT por User_ID)
                                       ▼
                        ┌────────────────────────────┐
                        │  Backend Core .NET Core     │
                        │  - Auth / OAuth2 LinkedIn   │
                        │  - Orquestación             │
                        │  - CRUD perfiles/vacantes   │
                        │  - Gestor dinámico de prompts│
                        └───┬──────────┬──────────┬───┘
                            │          │          │
                   ┌────────▼───┐ ┌────▼─────┐ ┌──▼──────────────┐
                   │ LLM Local  │ │ PostgreSQL│ │ Motor Búsqueda  │
                   │ (Llama/    │ │ (multi-   │ │ (scraping/APIs: │
                   │  Mistral)  │ │  tenant)  │ │ LinkedIn, Comp- │
                   │            │ │           │ │ trabajo, ElEmpleo)│
                   └────────────┘ └───────────┘ └─────────────────┘
                            │
                   ┌────────▼──────────────┐
                   │ Auditor ATS (Python)  │
                   │ NLP → % Match         │
                   └───────────────────────┘
```

**Sitemap (UI usuario final)**: `/login` → `/perfil` (ingesta) → `/vacantes` (búsqueda) → `/vacante/:id` (detalle + generar CV) → `/cv/:id` (match ATS + enriquecimiento) → descarga PDF/DOCX.

## 5. Características y funcionalidades

### Capability 1: Autenticación e Identidad (LinkedIn OAuth2)
- 1.1 Login exclusivo vía LinkedIn OAuth2 (cero contraseñas, cero registro manual).
- 1.2 Emisión de sesión/JWT ligada al `User_ID`.
- 1.3 Provisión automática de cuenta en primer login.

### Capability 2: Ingesta y Perfil Base
- 2.1 Extracción automática del perfil desde LinkedIn al hacer login.
- 2.2 Complemento opcional subiendo PDF base; el LLM local extrae info faltante en crudo (sin mega-formularios).
- 2.3 Almacenamiento del perfil estructurado en PostgreSQL, por `User_ID`.

### Capability 3: Motor de Búsqueda de Vacantes
- 3.1 Búsqueda bajo demanda por usuario (scraping o APIs: LinkedIn, Computrabajo, El Empleo).
- 3.2 Deduplicación: guarda `Vacante_ID × User_ID` para no repetir ofertas a la misma persona.
- 3.3 Persistencia permanente del historial de vacantes vistas por usuario.
- 3.4 Seguimiento de postulación: el usuario marca manualmente las vacantes a las que ya se postuló (envió el CV) y ve cuáles tiene pendientes por enviar. Es registro manual, no automatización del envío.

### Capability 4: Generación de CV Adaptado
- 4.1 Generación de CV adaptado a las palabras clave de la vacante (LLM local).
- 4.2 Exportación a PDF y DOCX.
- 4.3 Gestor dinámico de prompts parametrizados en PostgreSQL, editables en caliente.

### Capability 5: Auditoría ATS y Enriquecimiento
- 5.1 Auditor ATS en Python (NLP) que compara el CV generado con la vacante real y entrega un **% de Match exacto**.
- 5.2 Regla de negocio < 90%: si el match es menor, el sistema genera un formulario React para pedir la info faltante **o** sugiere descartar la vacante.
- 5.3 Persistencia permanente de decisiones (CV, match, descarte, enriquecimiento).

### Capability 6: Aislamiento Multi-tenant (transversal)
- 6.1 Todo registro pertenece a un `User_ID`; toda consulta filtra por él.
- 6.2 Pruebas de autorización que garantizan 0 fuga entre cuentas.

## 6. Diseño y experiencia del usuario

- **Principios UX**: flujo lineal guiado (login → perfil → vacante → CV → match), sin formularios largos; el LLM hace el trabajo pesado de captura. Feedback claro del % de match. "Don't make me think".
- **Accesibilidad**: WCAG AA como meta; navegación por teclado; contraste suficiente; idioma español.
- **Restricciones de marca**: sin marca corporativa definida (producto personal). Diseño limpio y funcional.
- **Inspiración / benchmarks**: Jobscan, Teal, Rezi (referencia de UX del match ATS; no copiar). `DESIGN_SOURCE` a definir en `/build:onboard`.

## 7. Requisitos técnicos

- **Stack**:
  - Frontend: **React** (Single Page Application).
  - Backend Core: **.NET Core** (APIs y orquestación).
  - IA: **LLM local** (Llama / Mistral) — inferencia on-premise.
  - Auditor ATS: **Python** (microservicio/scripts con librerías NLP).
  - Datos: **PostgreSQL**.
- **Integraciones**:
  - **LinkedIn OAuth2** (autenticación + extracción de perfil).
  - Fuentes de vacantes: **LinkedIn, Computrabajo, El Empleo** (scraping o API según disponibilidad).
- **No-funcionales**:
  - Performance: generación de CV + auditoría ATS end-to-end **< 5 min** (O1); búsqueda de vacantes responde en < 30 s.
  - Seguridad: OAuth2 (sin contraseñas propias); **aislamiento estricto por `User_ID`** en cada query; secretos server-side (client secret de LinkedIn, credenciales de BD) nunca en el frontend; datos personales (PII) tratados solo local (sin envío a IA de pago).
  - Disponibilidad: uso familiar; sin SLA formal. Datos **nunca efímeros** — persistencia permanente.
  - Observabilidad: logs de errores de scraping, de inferencia y de auditoría ATS. Destino: archivo local / stdout; retención: rolling 7 días.

## 8. Planificación del proyecto

> **Inicio**: 2026-07-31 (fecha de redacción del PRD). Fechas objetivo calculadas desde el inicio.

| Fase | Duración (est.) | Fecha objetivo | Hitos | Dependencias |
|---|---|---|---|---|
| F1 — Discovery | 1 semana | hasta 2026-08-07 | PRD, épicas, mapa, backlog priorizado | ninguna |
| F2 — Cimiento | 2 semanas | hasta 2026-08-21 | Auth LinkedIn, modelo de datos multi-tenant, esqueleto React/.NET | F1 |
| F3 — MVP núcleo | 3–4 semanas | hasta 2026-09-18 | Ingesta, búsqueda, generación CV, auditoría ATS, enriquecimiento | F2 |
| F4 — Producción | 1 semana | hasta 2026-09-25 | Hardening, pruebas de aislamiento, despliegue local | F3 |

## 9. Criterios de aceptación (nivel producto)

El producto se considera entregado cuando:
- [ ] Un usuario puede iniciar sesión con LinkedIn sin crear contraseña y obtener su perfil base autopoblado.
- [ ] Un usuario puede buscar vacantes en al menos un portal y no ve vacantes ya vistas.
- [ ] El sistema genera un CV adaptado a una vacante y lo exporta en PDF y DOCX.
- [ ] El auditor ATS entrega un % de match numérico contra la vacante.
- [ ] Cuando el match < 90%, el sistema ofrece formulario de enriquecimiento o sugerencia de descarte, y persiste la decisión.
- [ ] Una prueba de autorización confirma que un usuario no puede leer datos de otro (aislamiento multi-tenant).
- [ ] Los prompts del LLM se pueden editar en PostgreSQL y el cambio surte efecto sin re-desplegar.
- [ ] No se realiza ninguna llamada a APIs de IA de pago.

## 10. Apéndices y recursos adicionales

- Investigación de usuario: definición de producto v2 en `docs/00-input/definicion-producto.md`.
- Benchmarks: Jobscan, Teal, Rezi (UX de match ATS).
- Mockups / wireframes: pendientes (`DESIGN_SOURCE` a definir en `/build:onboard`).
- Referencias técnicas: OAuth2 LinkedIn, ecosistema LLM local (Ollama/llama.cpp), librerías NLP Python (spaCy, scikit-learn).

## 11. Non-goals (fuera de alcance)

Explícitamente NO se incluye en esta iteración:
- **Auto-aplicar**: no se automatiza el clic final de "Aplicar" en los portales externos. (El usuario sí puede **marcar manualmente** que ya se postuló para llevar el control — Cap 3.4 —; eso es registro, no automatización del envío.)
- **APIs de IA de pago**: no se usa ningún modelo de IA comercial de pago; solo LLM local.
- **Datos efímeros**: no se guardan datos de forma temporal — todo es persistente.
- **Panel/Interfaz de Administrador**: no hay UI admin. Gestión administrativa, limpieza de datos y ajustes globales se hacen directamente en PostgreSQL. La UI React es exclusivamente para el flujo del usuario final.
- **Registro público / onboarding masivo**: no hay signup abierto; es un sistema de usuarios cerrados (familia).

## 12. Métricas de éxito (KPIs)

| KPI | Línea base | Meta | Cuándo se mide |
|---|---|---|---|
| Tiempo vacante→CV descargado | ~30–45 min (manual) | < 5 min | Por sesión de generación |
| % de CVs con Match ATS ≥ 90% | ~30% (CV genérico) | ≥ 80% (tras ≤1 enriquecimiento) | Por CV auditado |
| Fugas de datos entre usuarios | n/a (nuevo) | 0 | Pruebas de autorización por release |
| Costo de inferencia IA / mes | $ variable (SaaS pago) | $0 | Mensual |
| % altas sin formulario manual | 0% | 100% (vía LinkedIn) | Por alta de usuario |

---

## Anexo A: PRD optimizado para coding agents

> Fases secuenciales para consumo directo del arnés de construcción (`@trycore/spec-build-harness`). Cada fase mapea a una o más épicas del backlog.

### Fase 1: Cimiento — Autenticación e Identidad
- **Dependencias**: ninguna
- **Resultado verificable**: un usuario inicia sesión con LinkedIn OAuth2, se provisiona su cuenta y recibe un token ligado a su `User_ID`. Sin contraseñas.
- **Alcance**: SÍ login OAuth2 LinkedIn, emisión de sesión/JWT, provisión de cuenta. NO extracción de perfil (Fase 3), NO UI de negocio.
- **Tiempo estimado del agente**: 10–15 min

### Fase 2: Cimiento — Modelo de Datos Multi-tenant
- **Dependencias**: Fase 1
- **Resultado verificable**: esquema PostgreSQL con todas las entidades ligadas a `User_ID`; capa de acceso a datos que filtra por tenant; prueba de aislamiento en verde.
- **Alcance**: SÍ esquema, migraciones, guardas de autorización por tenant. NO lógica de negocio.
- **Tiempo estimado del agente**: 10–15 min

### Fase 3: Ingesta y Perfil Base
- **Dependencias**: Fase 1, Fase 2
- **Resultado verificable**: al login se extrae el perfil de LinkedIn; opcionalmente se sube un PDF y el LLM local extrae info faltante; el perfil queda persistido por usuario.
- **Alcance**: SÍ extracción LinkedIn, ingesta PDF vía LLM local, persistencia. NO generación de CV.
- **Tiempo estimado del agente**: 12–15 min

### Fase 4: Motor de Búsqueda de Vacantes
- **Dependencias**: Fase 2
- **Resultado verificable**: búsqueda bajo demanda en ≥1 portal; deduplicación por `Vacante_ID × User_ID`; historial persistente.
- **Alcance**: SÍ scraping/API de un portal, dedup, persistencia. NO generación de CV.
- **Tiempo estimado del agente**: 12–15 min

### Fase 5: Generación de CV + Gestor de Prompts
- **Dependencias**: Fase 3, Fase 4
- **Resultado verificable**: dado un perfil + una vacante, el LLM local genera un CV adaptado exportable en PDF y DOCX; los prompts viven en PostgreSQL y son editables en caliente.
- **Alcance**: SÍ generación LLM, export PDF/DOCX, gestor de prompts en BD. NO auditoría ATS.
- **Tiempo estimado del agente**: 15 min

### Fase 6: Auditoría ATS + Enriquecimiento
- **Dependencias**: Fase 5
- **Resultado verificable**: el auditor Python calcula el % de match CV↔vacante; si < 90%, la UI React ofrece formulario de enriquecimiento o sugerencia de descarte; toda decisión se persiste.
- **Alcance**: SÍ microservicio Python NLP, cálculo de match, regla < 90%, formulario React, persistencia. Cierra el flujo end-to-end.
- **Tiempo estimado del agente**: 15 min
