import { beforeEach, describe, expect, it } from 'vitest'
import {
  clearSession,
  getToken,
  isAuthenticated,
  readTokenFromFragment,
  resolveSessionToken,
  saveToken,
} from './session'

describe('lectura del fragmento del callback', () => {
  it('extrae el token de un fragmento con #', () => {
    expect(readTokenFromFragment('#token=abc.def.ghi')).toBe('abc.def.ghi')
  })

  it('acepta el fragmento sin la almohadilla', () => {
    expect(readTokenFromFragment('token=abc.def.ghi')).toBe('abc.def.ghi')
  })

  it('devuelve null si no hay token en el fragmento', () => {
    expect(readTokenFromFragment('')).toBeNull()
    expect(readTokenFromFragment('#')).toBeNull()
    expect(readTokenFromFragment('#otra=cosa')).toBeNull()
    expect(readTokenFromFragment('#token=')).toBeNull()
  })

  it('conserva un token con caracteres codificados', () => {
    expect(readTokenFromFragment('#token=a.b%2Bc')).toBe('a.b+c')
  })
})

describe('almacenamiento de la sesión', () => {
  beforeEach(() => clearSession())

  it('guarda y recupera el token', () => {
    saveToken('jwt-1')
    expect(getToken()).toBe('jwt-1')
    expect(isAuthenticated()).toBe(true)
  })

  it('sin sesión no está autenticado', () => {
    expect(getToken()).toBeNull()
    expect(isAuthenticated()).toBe(false)
  })
})

// HU-021 — Cerrar sesión. La sesión vive en sessionStorage y el JWT es sin estado,
// así que "salir" es descartar la sesión del navegador.
describe('cierre de sesión (HU-021)', () => {
  beforeEach(() => clearSession())

  // AC1 — al salir, la sesión se descarta.
  it('cerrar sesión descarta el token', () => {
    saveToken('jwt-1')
    clearSession()
    expect(getToken()).toBeNull()
    expect(isAuthenticated()).toBe(false)
  })

  // AC2 — tras salir, el guard no debe dejar entrar al área autenticada: la señal
  // en la que se apoya (isAuthenticated) queda en falso.
  it('tras cerrar sesión no se puede volver al área autenticada', () => {
    saveToken('jwt-1')
    clearSession()
    expect(isAuthenticated()).toBe(false)
    // y tampoco se recupera "sola" al reevaluar el callback sin fragmento
    expect(resolveSessionToken('')).toBeNull()
  })

  // AC3 — la sesión no sobrevive al cierre de la pestaña. Se cumple por construcción
  // al usar sessionStorage (no localStorage): este test fija esa decisión para que un
  // cambio a localStorage no pase inadvertido.
  it('la sesión se guarda en sessionStorage, no en localStorage', () => {
    saveToken('jwt-1')
    expect(sessionStorage.getItem('empleabilidad.session.token')).toBe('jwt-1')
    expect(localStorage.getItem('empleabilidad.session.token')).toBeNull()
  })
})

// REGRESIÓN de un bug real detectado en el journey de EP-001: el manejo del
// callback consume el fragmento (lo borra de la URL) y, al ejecutarse una segunda
// vez — StrictMode en desarrollo, un remount o el botón atrás —, ya no encontraba
// el token y expulsaba al usuario al login pese a tener sesión válida.
describe('resolución de la sesión en el callback (idempotencia)', () => {
  beforeEach(() => clearSession())

  it('primera pasada: toma el token del fragmento', () => {
    expect(resolveSessionToken('#token=jwt-nuevo')).toBe('jwt-nuevo')
  })

  it('segunda pasada sin fragmento: reutiliza la sesión ya guardada', () => {
    saveToken('jwt-nuevo') // lo que dejó la primera pasada
    expect(resolveSessionToken('')).toBe('jwt-nuevo')
  })

  it('sin fragmento y sin sesión previa: no hay sesión', () => {
    expect(resolveSessionToken('')).toBeNull()
  })

  it('el fragmento tiene prioridad sobre una sesión anterior', () => {
    saveToken('jwt-viejo')
    expect(resolveSessionToken('#token=jwt-recien-emitido')).toBe('jwt-recien-emitido')
  })
})
