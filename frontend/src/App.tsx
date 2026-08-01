import { Link, Outlet, useNavigate } from 'react-router-dom'
import { clearSession } from './auth/session'

// Layout del área autenticada. Solo se renderiza tras el guard de sesión
// (RequireAuth); el login es una pantalla completa fuera de este layout.
// La navegación definitiva (rail del prototipo) llega con el design-system.
export function AppLayout() {
  const navigate = useNavigate()

  const salir = () => {
    clearSession()
    navigate('/login', { replace: true })
  }

  return (
    <div className="app">
      <header>
        <h1>Asistente de Empleabilidad IA</h1>
        <button className="btn-logout" onClick={salir}>
          Salir
        </button>
      </header>
      <nav>
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
