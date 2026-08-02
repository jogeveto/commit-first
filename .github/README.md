# CI

El pipeline vive en `ci-workflow.yml`. **Todavía no está activo**: para que GitHub lo
ejecute tiene que estar en `.github/workflows/ci.yml`, y subir un archivo a esa ruta
exige que el token tenga el scope `workflow` (el de esta cuenta no lo tiene).

Dos formas de activarlo, ambas de un minuto:

1. **Desde la web**: Actions → New workflow → set up a workflow yourself → pegar el
   contenido de `ci-workflow.yml` → guardar como `ci.yml`.
2. **Desde la terminal**: `gh auth refresh -h github.com -s workflow` (completando el
   flujo con la cuenta dueña del repositorio) y luego mover el archivo:
   `git mv .github/ci-workflow.yml .github/workflows/ci.yml`

Mientras no esté activo, las cuatro redes se corren a mano; los comandos están en el
propio `ci-workflow.yml`.
