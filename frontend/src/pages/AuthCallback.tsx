import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { resolveSessionToken, saveToken } from '../auth/session'

// HU-001 AC1 · Aterrizaje del callback OAuth2: el backend redirige aquí con la
// sesión en el fragmento (`#token=`). Se guarda y se entra al área autenticada.
// Sin token válido en el fragmento no hay sesión → vuelve al login (AC2/AC3).
export function AuthCallback() {
  const navigate = useNavigate()

  useEffect(() => {
    // Idempotente por diseño: ver `resolveSessionToken` (con test de regresión).
    const token = resolveSessionToken(window.location.hash)

    if (!token) {
      navigate('/login?error=sin_sesion&message=No%20se%20recibi%C3%B3%20una%20sesi%C3%B3n%20v%C3%A1lida.', {
        replace: true,
      })
      return
    }

    saveToken(token)
    // Limpia el fragmento para que el token no quede en la barra de direcciones
    // ni en el historial de navegación.
    window.history.replaceState(null, '', window.location.pathname)
    navigate('/perfil', { replace: true })
  }, [navigate])

  return (
    <section className="auth-callback">
      <p>Completando tu ingreso…</p>
    </section>
  )
}
