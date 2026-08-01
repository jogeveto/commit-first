import { beforeEach, describe, expect, it } from 'vitest'
import { api } from './client'
import { clearSession, isAuthenticated, saveToken } from '../auth/session'

// Interceptores del cliente HTTP: son el cableado entre la sesión del SPA y la API.
// Sin estos tests se podía dejar de enviar el Bearer (toda llamada protegida daría
// 401) o dejar de descartar la sesión ante un 401 (sesión zombi) con la suite verde.
//
// Se ejercitan los interceptores reales de la instancia `api`, sin servidor: se
// pasan la config y el error por las mismas funciones que axios invoca.

type Interceptor = { fulfilled?: (v: any) => any; rejected?: (e: any) => any }

function interceptoresDe(manager: any): Interceptor[] {
  return manager.handlers.filter(Boolean)
}

describe('cliente HTTP · sesión', () => {
  beforeEach(() => clearSession())

  // HU-003 AC1 — sin esto, ninguna petición protegida llevaría la sesión.
  it('adjunta el token de sesión como Bearer', async () => {
    saveToken('jwt-de-prueba')
    const [interceptor] = interceptoresDe((api.interceptors as any).request)

    const config = await interceptor.fulfilled!({ headers: {} as Record<string, string> })

    expect(config.headers.Authorization).toBe('Bearer jwt-de-prueba')
  })

  it('sin sesión no adjunta cabecera de autorización', async () => {
    const [interceptor] = interceptoresDe((api.interceptors as any).request)

    const config = await interceptor.fulfilled!({ headers: {} as Record<string, string> })

    expect(config.headers.Authorization).toBeUndefined()
  })

  // HU-003 — un 401 significa sesión inválida/expirada: se descarta para que el
  // guard devuelva al login en vez de dejar una sesión zombi.
  it('ante un 401 descarta la sesión', async () => {
    saveToken('jwt-de-prueba')
    const [interceptor] = interceptoresDe((api.interceptors as any).response)

    await interceptor.rejected!({ response: { status: 401 } }).catch(() => {})

    expect(isAuthenticated()).toBe(false)
  })

  // Un error que no es de autorización no debe cerrar la sesión del usuario.
  it('ante un 500 conserva la sesión', async () => {
    saveToken('jwt-de-prueba')
    const [interceptor] = interceptoresDe((api.interceptors as any).response)

    await interceptor.rejected!({ response: { status: 500 } }).catch(() => {})

    expect(isAuthenticated()).toBe(true)
  })
})
