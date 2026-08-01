import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'
import { Login } from './Login'
import { linkedInLoginUrl } from '../api/client'

// HU-001 — la pantalla de login. Cubre la cláusula terminal del AC2 ("vuelvo a la
// pantalla de login CON UN MENSAJE CLARO"), que hasta ahora solo se verificaba a
// nivel de redirect: que la URL llevara `message=`, no que la pantalla lo mostrara.
//
// Mutaciones verificadas: cambiar `{error && (` por `{false && (` (el aviso deja de
// renderizarse), y que el botón navegue a otro sitio.

function montar(query = '') {
  return render(
    <MemoryRouter initialEntries={[`/login${query}`]}>
      <Routes>
        <Route path="/login" element={<Login />} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('Login', () => {
  afterEach(cleanup)

  // HU-001 AC2 — el motivo del fallo se ve en pantalla, no solo en la URL.
  it('muestra el mensaje cuando la autenticación no se completó', () => {
    montar('?error=consent_rejected&message=No%20se%20complet%C3%B3%20la%20autenticaci%C3%B3n%20con%20LinkedIn.')

    const aviso = screen.getByRole('alert')
    expect(aviso.textContent).toContain('No se completó la autenticación con LinkedIn.')
  })

  // Ante un error sin texto, el usuario sigue viendo una explicación (no un hueco).
  it('con un error sin mensaje muestra una explicación por defecto', () => {
    montar('?error=sin_sesion')

    expect(screen.getByRole('alert').textContent).toMatch(/no se pudo completar la autenticación/i)
  })

  // Sin error no debe aparecer ningún aviso.
  it('sin error no muestra aviso', () => {
    montar()

    expect(screen.queryByRole('alert')).toBeNull()
  })

  // HU-001 AC1 — el botón inicia el flujo contra el backend (que genera el `state`).
  it('el botón lleva al inicio del flujo OAuth2 del backend', () => {
    montar()
    const asignada: string[] = []
    // `window.location` no es asignable en jsdom: se sustituye por un doble que
    // registra la navegación, que es lo que este test necesita observar.
    const original = window.location
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { set href(v: string) { asignada.push(v) }, get href() { return asignada[asignada.length - 1] ?? '' } },
    })

    try {
      screen.getByRole('button', { name: /Continuar con LinkedIn/ }).click()
    } finally {
      Object.defineProperty(window, 'location', { configurable: true, value: original })
    }

    expect(asignada).toEqual([linkedInLoginUrl])
  })
})
