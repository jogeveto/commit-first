import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { AppLayout } from './App'
import { clearSession, getToken, saveToken } from './auth/session'

// HU-021 AC1 — "cuando elijo la opción de salir, mi sesión se descarta y vuelvo a
// la pantalla de login". Probado sobre el CONTROL real (el botón del layout), no
// llamando a clearSession() desde el test: con eso último se podía dejar el botón
// sin cablear, o mandándolo a la pantalla equivocada, y la suite seguía verde.

function montarAreaAutenticada() {
  return render(
    <MemoryRouter initialEntries={['/perfil']}>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/perfil" element={<div>CONTENIDO_PRIVADO</div>} />
        </Route>
        <Route path="/login" element={<div>PANTALLA_LOGIN</div>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('AppLayout · cerrar sesión', () => {
  beforeEach(() => clearSession())
  afterEach(cleanup)

  it('el botón Salir descarta la sesión', async () => {
    saveToken('jwt-de-prueba')
    montarAreaAutenticada()

    screen.getByRole('button', { name: 'Salir' }).click()

    expect(getToken()).toBeNull()
  })

  it('el botón Salir lleva a la pantalla de login y deja de mostrar el contenido privado', async () => {
    saveToken('jwt-de-prueba')
    montarAreaAutenticada()
    expect(screen.getByText('CONTENIDO_PRIVADO')).toBeTruthy()

    screen.getByRole('button', { name: 'Salir' }).click()

    expect(await screen.findByText('PANTALLA_LOGIN')).toBeTruthy()
    expect(screen.queryByText('CONTENIDO_PRIVADO')).toBeNull()
  })
})
