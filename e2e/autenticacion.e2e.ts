import { expect, test } from '@playwright/test'

// EP-001 · journey de autenticación end-to-end sobre el stack real (SPA + API +
// PostgreSQL) en modo mock. Es el nivel que hasta ahora solo se verificaba a mano.
//
// Cada test declara la mutación que lo pone en rojo: si una no lo hace, el test
// no protege nada y hay que rehacerlo.

const API = process.env.E2E_API_URL ?? 'http://backend:8080'

test.beforeEach(async ({ context }) => {
  await context.clearCookies()
})

// HU-001 AC1 + HU-002 AC1 + HU-003 AC1 — el journey completo, de clic a identidad.
// Mutación: quitar el guard del router, romper el interceptor Bearer, o que el
// botón no inicie el flujo → rojo.
test('el usuario entra con LinkedIn y su sesión resuelve su User_ID', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByText('Entra en un clic')).toBeVisible()

  await page.getByRole('button', { name: /Continuar con LinkedIn/ }).click()

  await expect(page).toHaveURL(/\/perfil$/)
  const identidad = page.getByTestId('session-user-id')
  await expect(identidad).toBeVisible()
  // La identidad mostrada es un UUID real, no un texto de relleno.
  await expect(identidad).toContainText(/[0-9a-f]{8}-[0-9a-f]{4}-/)
})

// HU-021 AC1 — cerrar sesión devuelve al login.
// Mutación: quitar clearSession del handler, o mandar el botón a otra pantalla → rojo.
test('al salir se descarta la sesión y se vuelve al login', async ({ page }) => {
  await page.goto('/login')
  await page.getByRole('button', { name: /Continuar con LinkedIn/ }).click()
  await expect(page).toHaveURL(/\/perfil$/)

  await page.getByRole('button', { name: 'Salir' }).click()

  await expect(page).toHaveURL(/\/login$/)
  await expect(page.getByText('Entra en un clic')).toBeVisible()
})

// HU-021 AC2 / HU-003 — el área autenticada está protegida de verdad en la app.
// Mutación: borrar `element: <RequireAuth />` del router → rojo.
test('sin sesión, una ruta privada no muestra datos y lleva al login', async ({ page }) => {
  await page.goto('/perfil')

  await expect(page).toHaveURL(/\/login$/)
  await expect(page.getByTestId('session-user-id')).toHaveCount(0)
})

// HU-021 AC3 — la sesión no sobrevive al cierre de la pestaña (sessionStorage).
// Mutación: pasar a localStorage → rojo.
test('la sesión no sobrevive a una pestaña nueva', async ({ page, context }) => {
  await page.goto('/login')
  await page.getByRole('button', { name: /Continuar con LinkedIn/ }).click()
  await expect(page).toHaveURL(/\/perfil$/)

  const otraPestana = await context.newPage()
  await otraPestana.goto('/perfil')

  await expect(otraPestana).toHaveURL(/\/login$/)
  await otraPestana.close()
})

// HU-001 AC2 — el consentimiento rechazado devuelve al login CON el motivo visible.
// Mutación: `{false && (` en el aviso de Login.tsx → rojo.
test('si LinkedIn rechaza el consentimiento, el login explica por qué', async ({ page }) => {
  // El backend responde al callback con error redirigiendo al SPA: se recorre esa
  // misma ruta que seguiría el navegador del usuario.
  await page.goto('/login?error=consent_rejected&message=No%20se%20complet%C3%B3%20la%20autenticaci%C3%B3n%20con%20LinkedIn.')

  const aviso = page.getByRole('alert')
  await expect(aviso).toBeVisible()
  await expect(aviso).toContainText('No se completó la autenticación con LinkedIn.')
})

// HU-001 AC3 — un callback con `state` forjado no crea sesión (CSRF).
// Mutación: quitar la guarda de state en el servicio → rojo.
test('un callback con state forjado no deja entrar', async ({ page }) => {
  await page.goto(`${API}/auth/linkedin/callback?code=x&state=FORJADO`)

  await expect(page).toHaveURL(/\/login\?error=invalid_state/)
  await page.goto('/perfil')
  await expect(page).toHaveURL(/\/login$/)
})
