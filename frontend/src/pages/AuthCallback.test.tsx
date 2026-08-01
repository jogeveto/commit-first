import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { StrictMode } from 'react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { AuthCallback } from './AuthCallback'
import { clearSession, getToken } from '../auth/session'

// Tests del COMPONENTE, no solo de la función de sesión. El bug de EP-001 vivía en
// el efecto de AuthCallback: si alguien vuelve a leer el fragmento directamente en
// lugar de usar resolveSessionToken, los tests de session.ts seguirían en verde y el
// bug reaparecería. Estos tests montan el componente de verdad y observan a dónde
// navega, que es lo que sufre el usuario.

// Renderiza el callback con una URL dada y una pantalla destino observable.
function montar(hash: string, { estricto = false } = {}) {
  window.history.replaceState(null, '', `/auth/callback${hash}`)

  const arbol = (
    <MemoryRouter initialEntries={[`/auth/callback${hash}`]}>
      <Routes>
        <Route path="/auth/callback" element={<AuthCallback />} />
        <Route path="/perfil" element={<div>AREA_AUTENTICADA</div>} />
        <Route path="/login" element={<div>PANTALLA_LOGIN</div>} />
      </Routes>
    </MemoryRouter>
  )

  // StrictMode monta, desmonta y vuelve a montar: reproduce la doble ejecución de
  // efectos que destapó el bug original.
  return render(estricto ? <StrictMode>{arbol}</StrictMode> : arbol)
}

describe('AuthCallback', () => {
  beforeEach(() => clearSession())
  // vitest no registra el auto-cleanup de Testing Library (no usamos `globals`),
  // así que sin esto el DOM de un test se arrastra al siguiente y las búsquedas
  // encuentran elementos duplicados.
  afterEach(cleanup)

  // HU-001 AC1 — con sesión en el fragmento, entra al área autenticada.
  it('con token en el fragmento guarda la sesión y entra al área autenticada', async () => {
    montar('#token=jwt-de-prueba')

    expect(await screen.findByText('AREA_AUTENTICADA')).toBeTruthy()
    expect(getToken()).toBe('jwt-de-prueba')
  })

  // REGRESIÓN del bug real: bajo StrictMode el efecto corre dos veces y la segunda
  // ya no encuentra el fragmento (la primera lo consumió). Debe entrar igual.
  it('en doble montaje (StrictMode) no expulsa al login', async () => {
    montar('#token=jwt-de-prueba', { estricto: true })

    expect(await screen.findByText('AREA_AUTENTICADA')).toBeTruthy()
    await waitFor(() => expect(screen.queryByText('PANTALLA_LOGIN')).toBeNull())
    expect(getToken()).toBe('jwt-de-prueba')
  })

  // Sin sesión de ninguna procedencia, vuelve al login (no deja al usuario colgado).
  it('sin token y sin sesión previa vuelve al login', async () => {
    montar('')

    expect(await screen.findByText('PANTALLA_LOGIN')).toBeTruthy()
    expect(getToken()).toBeNull()
  })

  // El token no debe quedar en la barra de direcciones ni en el historial.
  it('limpia el fragmento de la URL tras consumirlo', async () => {
    montar('#token=jwt-de-prueba')

    await screen.findByText('AREA_AUTENTICADA')
    expect(window.location.hash).toBe('')
  })
})
