// Página stub del scaffold. Cada pantalla real se implementa en su slice, contra
// el prototipo clickeable (DESIGN_SOURCE). Aquí solo marca la ruta y su épica.
export function Placeholder({ title, epic }: { title: string; epic: string }) {
  return (
    <section className="placeholder">
      <h2>{title}</h2>
      <p>
        Pantalla del scaffold — pendiente de construir en <strong>{epic}</strong>.
      </p>
      <p className="hint">
        La UI real se desarrollará por slice, verificando fidelidad contra el prototipo clickeable.
      </p>
    </section>
  )
}
