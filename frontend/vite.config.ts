import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Scaffold del SPA. El dev server escucha en 0.0.0.0 para ser accesible desde el contenedor.
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
  },
})
