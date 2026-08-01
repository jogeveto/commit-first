import axios from 'axios'
import { clearSession, getToken } from '../auth/session'

// Cliente HTTP hacia la API .NET. La base URL se inyecta por variable de entorno
// (VITE_API_URL) — en docker-compose apunta al servicio backend.
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8080',
})

// EP-001 · adjunta la sesión (JWT por User_ID) a cada petición protegida.
api.interceptors.request.use((config) => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// HU-003 · un 401 significa sesión ausente/inválida/expirada: se descarta la sesión
// local para que el guard devuelva al login en vez de dejar una UI a medias.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) clearSession()
    return Promise.reject(error)
  },
)

/// URL del inicio del flujo OAuth2 (el backend genera el `state` CSRF y redirige).
export const linkedInLoginUrl = `${api.defaults.baseURL}/auth/linkedin/start`
