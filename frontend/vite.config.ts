/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Scaffold del SPA. El dev server escucha en 0.0.0.0 para ser accesible desde el contenedor.
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
    // Vite rechaza peticiones cuyo Host no reconoce (protección contra DNS
    // rebinding). Dentro de la red de compose el navegador del E2E pide
    // `http://frontend:5173`, así que hay que permitir ese nombre de servicio
    // además de localhost; si no, devuelve "Blocked request" y la SPA no carga.
    allowedHosts: ['localhost', 'frontend'],
    // En bind mounts de Windows/macOS los eventos de fichero no llegan al
    // contenedor: sin sondeo, Vite sirve una versión CACHEADA y se acaba probando
    // código que ya no existe. Nos pasó: una mutación llegó al contenedor y el
    // navegador seguía viendo el código anterior.
    watch: { usePolling: true, interval: 300 },
  },
  // `vite preview` sirve el build de producción: es lo que ejercita el E2E, para
  // no depender del dev server ni de su caché.
  preview: {
    host: true,
    port: 5173,
    allowedHosts: ['localhost', 'frontend'],
  },
  // Pruebas del SPA con vitest. Entorno jsdom porque la lógica de sesión usa
  // sessionStorage y la URL del navegador (fragmento del callback OAuth2).
  test: {
    environment: 'jsdom',
    include: ['src/**/*.test.ts', 'src/**/*.test.tsx'],
  },
})
