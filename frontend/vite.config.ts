/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Scaffold del SPA. El dev server escucha en 0.0.0.0 para ser accesible desde el contenedor.
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
  },
  // Pruebas del SPA con vitest. Entorno jsdom porque la lógica de sesión usa
  // sessionStorage y la URL del navegador (fragmento del callback OAuth2).
  test: {
    environment: 'jsdom',
    include: ['src/**/*.test.ts', 'src/**/*.test.tsx'],
  },
})
