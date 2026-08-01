import axios from 'axios'

// Cliente HTTP hacia la API .NET. La base URL se inyecta por variable de entorno
// (VITE_API_URL) — en docker-compose apunta al servicio backend.
// El token de sesión (JWT por User_ID, EP-001) se agregará como interceptor en ese slice.
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8080',
})
