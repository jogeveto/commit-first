import { cleanup, render, screen } from '@testing-library/react'
import { RouterProvider, createMemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { routes } from './router'
import { clearSession, saveToken } from './auth/session'

// RAÍZ DE COMPOSICIÓN. Estos tests montan las rutas REALES de la aplicación, no un
// router inventado para el test. Sin ellos se podía quitar el guard o el layout de
// router.tsx —dejando el área autenticada abierta— con toda la suite en verde:
// los tests de componente probaban las piezas, no que la app las use.
//
// Mutaciones que los ponen en rojo (verificadas):
//   · quitar `element: <RequireAuth />` de las rutas protegidas
//   · quitar `element: <AppLayout />`
//   · apuntar /login a otra pantalla

function montarEn(ruta: string) {
  const router = createMemoryRouter(routes, { initialEntries: [ruta] })
  return render(<RouterProvider router={router} />)
}

describe('router de la aplicación', () => {
  beforeEach(() => clearSession())
  afterEach(cleanup)

  // HU-021 AC2 / HU-003 — el área autenticada está realmente protegida en la app.
  it('sin sesión, una ruta privada lleva al login', async () => {
    montarEn('/perfil')

    expect(await screen.findByText('Entra en un clic')).toBeTruthy()
    expect(screen.queryByText('Mi perfil')).toBeNull()
  })

  // HU-001 AC1 — con sesión se entra al área autenticada, con su layout.
  it('con sesión, la ruta privada muestra el área autenticada y el botón de salir', async () => {
    saveToken('jwt-de-prueba')

    montarEn('/perfil')

    expect(await screen.findByText('Mi perfil')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Salir' })).toBeTruthy()
  })

  // HU-001 — /login es la puerta de entrada y muestra la pantalla de login.
  it('/login muestra la pantalla de login', async () => {
    montarEn('/login')

    expect(await screen.findByText('Entra en un clic')).toBeTruthy()
    expect(screen.getByText('Continuar con LinkedIn')).toBeTruthy()
  })

  // La raíz redirige al área autenticada (y el guard decide si se puede pasar).
  it('sin sesión, la raíz acaba en el login', async () => {
    montarEn('/')

    expect(await screen.findByText('Entra en un clic')).toBeTruthy()
  })
})
