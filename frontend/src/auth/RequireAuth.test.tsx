import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { RequireAuth } from './RequireAuth'
import { clearSession, saveToken } from './session'

// HU-021 AC2 / HU-003 — el guard del área autenticada, probado sobre el COMPONENTE.
// Los tests de session.ts no bastaban: se podía desactivar el guard por completo
// (`if (false)`) y toda la suite seguía en verde.

function montar() {
  return render(
    <MemoryRouter initialEntries={['/perfil']}>
      <Routes>
        <Route element={<RequireAuth />}>
          <Route path="/perfil" element={<div>CONTENIDO_PRIVADO</div>} />
        </Route>
        <Route path="/login" element={<div>PANTALLA_LOGIN</div>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('RequireAuth', () => {
  beforeEach(() => clearSession())
  afterEach(cleanup)

  it('sin sesión no muestra el contenido privado y lleva al login', () => {
    montar()

    expect(screen.queryByText('CONTENIDO_PRIVADO')).toBeNull()
    expect(screen.getByText('PANTALLA_LOGIN')).toBeTruthy()
  })

  it('con sesión deja pasar al contenido privado', () => {
    saveToken('jwt-de-prueba')

    montar()

    expect(screen.getByText('CONTENIDO_PRIVADO')).toBeTruthy()
    expect(screen.queryByText('PANTALLA_LOGIN')).toBeNull()
  })
})
