import { useSearchParams } from 'react-router-dom'
import { linkedInLoginUrl } from '../api/client'

// HU-001 · Pantalla de login (única puerta de entrada: sin contraseñas ni registro).
// Fiel al prototipo clickeable (DESIGN_SOURCE docs/07-prototipo/index.html #login):
// dos columnas — brand-side verde pino + tarjeta con "Continuar con LinkedIn".
export function Login() {
  const [params] = useSearchParams()
  const error = params.get('error')
  const message = params.get('message')

  // El backend genera el `state` (CSRF) en /auth/linkedin/start; por eso el botón
  // navega al backend en vez de construir la URL de LinkedIn en el cliente
  // (el client secret y el state jamás viven en el frontend).
  const iniciarSesion = () => {
    window.location.href = linkedInLoginUrl
  }

  return (
    <section id="login">
      <div className="brand-side">
        <div className="brand-mark">
          <span className="dot">A</span>
          <b>Afina · Empleabilidad IA</b>
        </div>
        <div className="brand-hero">
          <div className="eyebrow">Hoja de vida · a la medida del ATS</div>
          <h1>
            Tu experiencia,
            <br />
            <em>afinada</em> para cada vacante.
          </h1>
          <p>
            Una hoja de vida distinta para cada oferta, con tu porcentaje de match real antes de
            postularte. Todo con IA local: tus datos nunca salen de aquí.
          </p>
        </div>
        <div className="brand-foot">
          <span>
            <i className="ic" /> IA 100% local
          </span>
          <span>
            <i className="ic" /> Datos aislados por persona
          </span>
          <span>
            <i className="ic" /> Sin costos de IA
          </span>
        </div>
      </div>

      <div className="form-side">
        <div className="login-card">
          <div className="kicker">Bienvenido de nuevo</div>
          <h2>Entra en un clic</h2>
          <p>Sin contraseñas ni formularios de registro. Tu perfil se arma solo desde LinkedIn.</p>

          {/* HU-001 AC2/AC3 · el motivo del fallo se explica sin jerga técnica */}
          {error && (
            <div className="login-error" role="alert">
              <span aria-hidden="true">⚠️</span>
              <span>{message || 'No se pudo completar la autenticación. Intenta de nuevo.'}</span>
            </div>
          )}

          <button className="btn-linkedin" onClick={iniciarSesion}>
            <svg viewBox="0 0 24 24" fill="#fff" aria-hidden="true">
              <path d="M20.45 20.45h-3.55v-5.57c0-1.33-.02-3.04-1.85-3.04-1.85 0-2.13 1.45-2.13 2.94v5.67H9.36V9h3.41v1.56h.05c.47-.9 1.63-1.85 3.36-1.85 3.6 0 4.27 2.37 4.27 5.45v6.29zM5.34 7.43a2.06 2.06 0 1 1 0-4.12 2.06 2.06 0 0 1 0 4.12zM7.12 20.45H3.55V9h3.57v11.45zM22.22 0H1.77C.79 0 0 .77 0 1.72v20.56C0 23.23.79 24 1.77 24h20.45c.98 0 1.78-.77 1.78-1.72V1.72C24 .77 23.2 0 22.22 0z" />
            </svg>
            Continuar con LinkedIn
          </button>

          <div className="login-note">
            <span style={{ fontSize: 15 }}>🔒</span>
            <span>
              Al entrar, extraemos tu perfil de LinkedIn para armar tu base. Podrás complementarlo
              con un PDF; la IA local lee lo que falte.
            </span>
          </div>
          <div className="login-fine">
            Uso familiar cerrado · Ana y Diego tienen cuentas totalmente independientes.
            <br />
            No hay panel de administrador: la gestión se hace sobre la base de datos.
          </div>
        </div>
      </div>
    </section>
  )
}
