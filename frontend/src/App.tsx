import { Link, Outlet } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { api } from './api/client'

// Layout base del scaffold: navegación por el sitemap + verificación de wiring con la API.
export function AppLayout() {
  const [apiStatus, setApiStatus] = useState<string>('comprobando…')

  useEffect(() => {
    api.get('/health')
      .then((r) => setApiStatus(`API: ${r.data.status}`))
      .catch(() => setApiStatus('API: sin conexión'))
  }, [])

  return (
    <div className="app">
      <header>
        <h1>Asistente de Empleabilidad IA</h1>
        <small>{apiStatus}</small>
      </header>
      <nav>
        <Link to="/login">Login</Link>
        <Link to="/perfil">Perfil</Link>
        <Link to="/vacantes">Vacantes</Link>
        <Link to="/vacante/demo">Generar CV</Link>
        <Link to="/cv/demo">Auditoría ATS</Link>
      </nav>
      <main>
        <Outlet />
      </main>
    </div>
  )
}
