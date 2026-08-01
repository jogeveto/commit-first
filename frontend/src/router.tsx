import { createBrowserRouter, Navigate } from 'react-router-dom'
import { AppLayout } from './App'
import { Placeholder } from './pages/Placeholder'
import { Login } from './pages/Login'
import { AuthCallback } from './pages/AuthCallback'
import { Perfil } from './pages/Perfil'
import { RequireAuth } from './auth/RequireAuth'

// Router del sitemap del PRD (§4): login → perfil → vacantes → vacante/:id → cv/:id.
// EP-001 construye login + callback + guard de sesión; las demás pantallas siguen
// como stubs hasta su slice, contra el prototipo clickeable (DESIGN_SOURCE).
//
// Login y callback viven FUERA del AppLayout: el login es una pantalla completa
// (fiel al prototipo) y no debe mostrar la navegación del área autenticada.
// Las rutas se exportan aparte del router para poder montarlas en tests con un
// router en memoria. Sin esto, la RAÍZ DE COMPOSICIÓN quedaba sin cubrir: se podía
// quitar el guard o el layout de aquí y toda la suite seguía en verde, porque los
// tests de componente montan su propio router en vez de estas rutas.
export const routes = [
  { path: '/login', element: <Login /> },
  { path: '/auth/callback', element: <AuthCallback /> },
  {
    path: '/',
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <Navigate to="/perfil" replace /> },
          { path: 'perfil', element: <Perfil /> },
          { path: 'vacantes', element: <Placeholder title="Buscar vacantes" epic="EP-004" /> },
          { path: 'vacante/:id', element: <Placeholder title="Detalle de vacante · Generar CV" epic="EP-005" /> },
          { path: 'cv/:id', element: <Placeholder title="Auditoría ATS · Match y enriquecimiento" epic="EP-006" /> },
        ],
      },
    ],
  },
]

export const router = createBrowserRouter(routes)
