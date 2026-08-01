// Sesión del SPA (EP-001, HU-003). El JWT lo emite el backend tras el callback
// OAuth2 y llega en el FRAGMENTO de la URL (`#token=`), que nunca viaja al servidor.
//
// Se guarda en sessionStorage: muere al cerrar la pestaña y no se comparte entre
// pestañas, reduciendo la ventana de exposición frente a localStorage. La clave de
// firma sigue siendo server-side; aquí solo vive la sesión ya emitida.
// Trade-off registrado: un XSS podría leer el token (inherente a Bearer en SPA) —
// a evaluar por el security-reviewer en el Release Gate.

const STORAGE_KEY = 'empleabilidad.session.token'

export function saveToken(token: string): void {
  sessionStorage.setItem(STORAGE_KEY, token)
}

export function getToken(): string | null {
  return sessionStorage.getItem(STORAGE_KEY)
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function isAuthenticated(): boolean {
  return getToken() !== null
}

/// Extrae el token del fragmento `#token=...` de la URL del callback.
export function readTokenFromFragment(hash: string): string | null {
  const fragment = hash.startsWith('#') ? hash.slice(1) : hash
  const token = new URLSearchParams(fragment).get('token')
  return token && token.length > 0 ? token : null
}

/// Resuelve la sesión al aterrizar en el callback: primero el token recién emitido
/// que llega en el fragmento; si no está, la sesión ya guardada.
///
/// Ese respaldo es lo que hace la operación IDEMPOTENTE. El manejo del callback
/// consume el fragmento (lo borra de la URL), de modo que una segunda ejecución
/// —StrictMode en desarrollo, un remount o el botón atrás— no lo encontraría y
/// expulsaría al usuario al login pese a tener sesión válida. Fue un bug real de
/// EP-001; los tests de `session.test.ts` lo cubren para que no reaparezca.
export function resolveSessionToken(hash: string): string | null {
  return readTokenFromFragment(hash) ?? getToken()
}
