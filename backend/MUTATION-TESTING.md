# Mutation testing del backend

## Por qué existe

La suite verifica el producto, pero **nada verificaba la suite**. Cinco auditorías
independientes seguidas encontraron asserts que **no podían fallar**: comprobaban algo
en un sitio donde ningún cambio del código de producción los pondría en rojo. Un
assert así es peor que no tener ninguno, porque aparenta cobertura y desactiva la
vigilancia de quien venga después.

Stryker responde a la única pregunta que importa sobre un test: **¿qué cambio en el
código lo pondría en rojo?** Si ninguno, el test no protege nada. Al romper el build
por debajo del umbral, "matar la mutación" deja de depender de la disciplina de quien
escribe y pasa a ser una garantía del sistema.

## No se excluye nada

Una versión previa de este archivo excluía `Program.cs` con el argumento de que "ya lo
cubren los tests de integración y el contrato". **Una auditoría lo falsificó
ejecutando**: ahí dentro sobrevivían mutaciones graves.

- inyectar el cliente **simulado** teniendo credenciales reales → cualquier `code`
  produciría una identidad válida (suplantación trivial en producción);
- construir la URL de autorización **sin `state`** → defensa CSRF desactivada;
- ampliar la sesión de 8 horas a 365 días;
- desactivar la validación del emisor del token;
- abrir CORS a cualquier origen, revirtiendo un fix anterior.

Excluir el archivo donde vive el riesgo hace que el gate **certifique justo lo que no
mide**. Por eso hoy no hay exclusiones, y la lógica que antes vivía en `Program.cs`
(la selección de proveedor y la construcción de la URL de autorización) se extrajo a
`Auth/LinkedInOAuthOptions.cs`, donde es verificable.

## El umbral es un trinquete, no un deseo

Medición del 2026-08-01, sin exclusiones: **53,89 %** (97 muertos / 49 supervivientes).
El umbral `break` está en **50**: por debajo del score real, para que el build falle
solo cuando la calidad **baja**.

Poner aquí una cifra aspiracional (85) tendría el efecto contrario al buscado: el
build nace en rojo, alguien baja el umbral o desactiva el gate, y la red desaparece.
La regla es **de trinquete**: ningún slice puede dejar el score por debajo del
umbral, y cuando el score real suba de forma estable, **se sube el umbral con él**.

Los supervivientes que quedan se concentran en `Program.cs` (cableado de DI,
serialización de respuestas y ramas que solo recorre el proveedor real de LinkedIn,
que no se puede automatizar porque exige consentimiento humano). El journey E2E
cubre por fuera la parte de arranque; Stryker no lo ejecuta.

## Cómo ejecutarlo

Requiere la red de compose: hay tests de integración contra PostgreSQL.

```bash
docker compose -f docker-compose.yml -f docker-compose.mock.yml up -d db backend

docker run --rm --network my-top-profile_default \
  -v "//c/Users/.../my-top-profile/backend://src" -w //src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  sh -c "dotnet tool restore && dotnet stryker"
```

El informe HTML queda en `StrykerOutput/` (ignorado por git). El build falla si el
score baja del umbral `break` de `stryker-config.json`.
