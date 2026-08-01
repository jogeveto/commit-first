# Definición del Producto - Asistente de Empleabilidad IA (Versión Final v2)

## 1. El Problema (El "Por qué")
Superar los filtros automáticos (ATS) de los portales de empleo requiere adaptar la hoja de vida para cada vacante. Hacerlo manualmente es lento, tedioso y no garantiza el éxito.

## 2. El Usuario Objetivo (El "Quién")
Usuarios cerrados / Familia (Ej. Tú y tu esposa). El sistema debe ser capaz de manejar múltiples usuarios de forma completamente independiente.

## 3. Arquitectura, Seguridad y Stack Tecnológico
* **Frontend:** React (Single Page Application).
* **Backend Core:** .NET Core (APIs y orquestación).
* **Autenticación (OAuth2):** Inicio de sesión exclusivo mediante LinkedIn (Cero contraseñas, cero registros manuales).
* **Aislamiento de Datos (Multi-tenancy):** Arquitectura estricta donde cada registro en base de datos pertenece a un usuario. Las ofertas vistas, historial de scraping y perfiles son 100% independientes y no se cruzan entre cuentas.
* **IA (El Cerebro):** Modelo LLM Local (Ej. Llama/Mistral) para garantizar privacidad y evitar costos.
* **Auditor ATS:** Microservicio/Scripts en Python con librerías NLP.
* **Base de Datos:** PostgreSQL.

## 4. Las Funcionalidades Core (El "Qué")
* **Ingesta vía LinkedIn + Documento:** Al hacer login con LinkedIn, el sistema extrae automáticamente la información del perfil para crear la base. Se puede complementar subiendo un PDF base donde el LLM local extrae la info faltante en crudo (sin sesgos ni mega-formularios manuales).
* **Gestor Dinámico de Prompts:** Los prompts que usa el LLM están parametrizados en PostgreSQL. Se pueden editar en caliente.
* **Motor de Búsqueda Personalizado:** Scraping o consumo de APIs (LinkedIn, Computrabajo, El Empleo) que busca vacantes bajo demanda por usuario. Guarda los IDs de vacantes cruzados con el User_ID para no repetir ofertas a la misma persona.
* **Generador de CVs Únicos:** Crea hojas de vida adaptadas a las palabras clave de la vacante, exportables en PDF y DOCX.
* **Auditor ATS (Validación Python):** Escanea el PDF generado y lo compara con la vacante real, entregando un % de Match exacto.
* **Enriquecimiento Dinámico (Regla de < 90%):** Si el match es menor al 90%, el sistema genera un formulario en React para pedir al usuario la información faltante o sugiere descartar la vacante (guardando todo permanentemente).

## 5. Lo que NO vamos a hacer (Límites del MVP)
* No hay automatización del clic final de "Aplicar" en los portales externos.
* No se usarán APIs de IA de pago.
* No se guardarán datos de forma efímera.
* **No habrá Interfaz/Panel de Administrador (Nuevo):** Toda la gestión administrativa, limpieza de datos o ajustes globales se hará directamente interactuando con la base de datos (PostgreSQL). La UI de React es exclusivamente para el flujo del usuario final.
