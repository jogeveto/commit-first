import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { isAuthenticated } from './session'

// HU-003 · Guard de rutas del SPA: sin sesión no se entra al área autenticada.
// Es la contraparte de cliente del middleware del backend (que es la autoridad
// real: aunque alguien fuerce la ruta, la API responde 401 sin exponer datos).
export function RequireAuth() {
  const location = useLocation()

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}
