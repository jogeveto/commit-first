import { useEffect, useState } from 'react'
import { api } from '../api/client'

// Área autenticada (destino del login, HU-001 AC1). El contenido real del perfil
// se construye en EP-003; aquí se verifica el cableado de la sesión end-to-end:
// el SPA llama al endpoint protegido /api/me con el Bearer y muestra su User_ID
// (HU-003 AC1 — la sesión resuelve la identidad del tenant).
export function Perfil() {
  const [userId, setUserId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .get('/api/me')
      .then((r) => setUserId(r.data.userId))
      .catch(() => setError('No se pudo verificar tu sesión.'))
  }, [])

  return (
    <section className="placeholder">
      <h2>Mi perfil</h2>
      {userId && (
        <p data-testid="session-user-id">
          Sesión activa · User_ID: <strong>{userId}</strong>
        </p>
      )}
      {error && <p role="alert">{error}</p>}
      <p className="hint">
        El contenido del perfil (extracción de LinkedIn + PDF) se construye en <strong>EP-003</strong>.
      </p>
    </section>
  )
}
