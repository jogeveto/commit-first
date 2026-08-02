import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { Perfil } from './Perfil'
import { api } from '../api/client'
import { clearSession, saveToken } from '../auth/session'

// HU-003 AC1 en el SPA. Perfil es el ÚNICO consumidor real de un endpoint
// protegido, y no tenía test: se podía borrar la llamada a /api/me y mostrar un
// UUID inventado con toda la suite en verde. La identidad mostrada al usuario
// debe venir del backend, no del propio cliente.
//
// Se intercepta en el adaptador HTTP de axios (no se sustituye `api`), así que se
// ejercitan los interceptores reales.

type Peticion = { url?: string; headers?: Record<string, string> }

function conRespuesta(status: number, data: unknown) {
  const vistas: Peticion[] = []
  const original = api.defaults.adapter
  api.defaults.adapter = async (config: any) => {
    vistas.push({ url: config.url, headers: config.headers })
    if (status >= 400) {
      const error: any = new Error(`HTTP ${status}`)
      error.response = { status, data, config, headers: {} }
      throw error
    }
    return { data, status, statusText: 'OK', headers: {}, config }
  }
  return { vistas, restaurar: () => { api.defaults.adapter = original } }
}

describe('Perfil', () => {
  beforeEach(() => clearSession())
  afterEach(cleanup)

  // Muestra el User_ID QUE DEVUELVE EL BACKEND, no uno cualquiera.
  it('pide /api/me y muestra el User_ID que responde el backend', async () => {
    saveToken('jwt-de-prueba')
    const { vistas, restaurar } = conRespuesta(200, { userId: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee' })

    try {
      render(<Perfil />)
      const identidad = await screen.findByTestId('session-user-id')
      expect(identidad.textContent).toContain('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee')
    } finally {
      restaurar()
    }

    expect(vistas.map((v) => v.url)).toEqual(['/api/me'])
  })

  // Si la sesión no sirve, no se inventa una identidad: se avisa.
  it('si la petición falla no muestra identidad y avisa', async () => {
    saveToken('jwt-de-prueba')
    const { restaurar } = conRespuesta(401, {})

    try {
      render(<Perfil />)
      expect(await screen.findByRole('alert')).toBeTruthy()
      expect(screen.queryByTestId('session-user-id')).toBeNull()
    } finally {
      restaurar()
    }
  })
})
