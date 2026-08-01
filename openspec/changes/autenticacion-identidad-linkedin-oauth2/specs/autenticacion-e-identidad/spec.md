## ADDED Requirements

### Requirement: Autenticación vía LinkedIn OAuth2
El sistema SHALL permitir el inicio de sesión exclusivamente mediante LinkedIn OAuth2 (Authorization Code), sin contraseñas ni registro manual, y SHALL validar el parámetro `state` para proteger contra CSRF. El client secret de LinkedIn MUST permanecer server-side (nunca expuesto al frontend). Trazabilidad: HU-001.

#### Scenario: Autenticación exitosa emite sesión
- **WHEN** un usuario sin sesión autoriza el acceso en el flujo OAuth2 de LinkedIn y el callback llega con un `state` válido
- **THEN** el sistema completa la autenticación, emite una sesión ligada a su `User_ID` y lo redirige a su área autenticada

#### Scenario: Consentimiento rechazado no crea sesión
- **WHEN** el usuario rechaza el consentimiento o LinkedIn devuelve un error de autorización
- **THEN** el sistema retorna a la pantalla de login con un mensaje claro de que la autenticación no se completó y NO crea sesión

#### Scenario: Callback con state inválido es rechazado (CSRF)
- **WHEN** el callback de LinkedIn llega con un `state` ausente o que no coincide con el emitido al iniciar el flujo
- **THEN** el sistema rechaza la autenticación, NO crea sesión y registra el intento

### Requirement: Provisión automática e idempotente de cuenta
El sistema SHALL provisionar automáticamente la cuenta del usuario en el primer acceso autenticado, usando la identidad estable de LinkedIn (`linkedin_sub`) como clave natural, y SHALL ser idempotente: accesos recurrentes reutilizan el `User_ID` existente sin duplicar. Trazabilidad: HU-002.

#### Scenario: Primer acceso crea la cuenta
- **WHEN** un usuario se autentica con LinkedIn y no existe cuenta para su `linkedin_sub`
- **THEN** el sistema crea su cuenta con un `User_ID` nuevo y lo deja con sesión activa

#### Scenario: Fallo al persistir no deja cuenta parcial
- **WHEN** ocurre un error al guardar la cuenta durante el primer acceso
- **THEN** el sistema NO emite sesión y NO deja una cuenta parcial creada, informando que no se pudo completar el alta

#### Scenario: Acceso recurrente no duplica la cuenta
- **WHEN** un usuario con cuenta ya provisionada se autentica de nuevo
- **THEN** el sistema reutiliza su `User_ID` existente y NO crea una cuenta duplicada

### Requirement: Sesión ligada al User_ID y autorización de peticiones
El sistema SHALL emitir una sesión/JWT firmada server-side que porta el `User_ID`, y un middleware SHALL resolver ese `User_ID` en el contexto de cada petición protegida. Peticiones sin token, con token inválido/manipulado o expirado MUST recibir 401 sin ejecutar ninguna operación ni exponer datos. Trazabilidad: HU-003.

#### Scenario: Petición autenticada resuelve el User_ID
- **WHEN** un cliente llama a un endpoint protegido con un token de sesión válido
- **THEN** el backend resuelve el `User_ID` del token y responde con los datos de ese usuario

#### Scenario: Token ausente o inválido retorna 401
- **WHEN** un cliente llama a un endpoint protegido sin token o con un token manipulado
- **THEN** el sistema responde 401 No autorizado y NO ejecuta ninguna operación

#### Scenario: Token expirado retorna 401
- **WHEN** un cliente llama a un endpoint protegido con un token cuya expiración ya venció
- **THEN** el sistema responde 401 con indicación de re-autenticar y NO expone datos
