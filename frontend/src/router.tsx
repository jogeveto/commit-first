import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from './App'
import { Placeholder } from './pages/Placeholder'

// Router del sitemap del PRD (§4): login → perfil → vacantes → vacante/:id → cv/:id.
// Cada ruta es un stub del scaffold; la UI real de cada pantalla se construye en su
// slice (EP-001 login, EP-003 perfil, EP-004 vacantes, EP-005 cv, EP-006 auditoría),
// contra el prototipo clickeable (DESIGN_SOURCE) como fuente de fidelidad.
export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/login" replace /> },
      { path: 'login', element: <Placeholder title="Login con LinkedIn" epic="EP-001" /> },
      { path: 'perfil', element: <Placeholder title="Mi perfil" epic="EP-003" /> },
      { path: 'vacantes', element: <Placeholder title="Buscar vacantes" epic="EP-004" /> },
      { path: 'vacante/:id', element: <Placeholder title="Detalle de vacante · Generar CV" epic="EP-005" /> },
      { path: 'cv/:id', element: <Placeholder title="Auditoría ATS · Match y enriquecimiento" epic="EP-006" /> },
    ],
  },
])
