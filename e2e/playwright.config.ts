import { defineConfig, devices } from '@playwright/test'

// E2E contra el stack REAL levantado con docker compose en MODO MOCK.
//
// Por qué existe: hasta ahora el nivel más externo —la aplicación ejercitada como
// la usa una persona— solo se verificaba a mano. Cuatro auditorías seguidas
// encontraron defectos que vivían justo ahí: por encima de la costura automatizada
// más alta (guard de rutas sin cablear, botón sin conectar, aviso que no se
// renderiza). Un test manual demuestra, pero no protege contra regresiones.
//
// Uso:
//   docker compose -f docker-compose.yml -f docker-compose.mock.yml up -d
//   docker compose run --rm e2e
export default defineConfig({
  testDir: '.',
  testMatch: '**/*.e2e.ts',
  // Sin reintentos: un test E2E que solo pasa a veces es ruido, no una red.
  retries: 0,
  timeout: 30_000,
  expect: { timeout: 10_000 },
  reporter: [['list']],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? 'http://frontend:5173',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
})
