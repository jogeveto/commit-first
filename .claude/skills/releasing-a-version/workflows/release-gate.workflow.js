// =============================================================================
// PLANTILLA — referencia, no obligatoria. Adapta los nombres de agente y el rango de
// diff a tu proyecto. El arnés es AGNÓSTICO: este archivo NO debe contener vocabulario
// de dominio ni de cliente — lo verifica scripts/check-agnostic.sh (incluye *.js).
//
// release-gate.workflow.js — Conducción del Release Gate (outer loop), §5 de METODOLOGIA.md.
// Hogar PRIMARIO de los workflows: corre UNA vez por release (O(releases)), fuera del camino
// caliente del inner loop. NO duplica el inner loop (ni TDD ni gates por slice).
//
// READ-ONLY sobre el estado: esta plantilla devuelve un diagnóstico; la skill
// releasing-a-version es la ÚNICA que escribe releases[] (una entrada por release, validando
// contra build-state.schema.json antes de persistir, updated_by:releasing-a-version).
// Parciales NO promueven a passed. Si METODOLOGIA.md (§5) contradice algo aquí, gana la metodología.
//
// RUNTIME: corre en el runtime de Workflow de Claude Code, que provee los globals
// agent()/parallel()/pipeline()/phase()/log()/args y envuelve el cuerpo en un contexto async
// (por eso usa `await` y `return` a nivel superior). NO es un módulo node standalone.
// =============================================================================

export const meta = {
  name: 'release-gate',
  description: 'Reviewers pesados en paralelo + integración secuencial + síntesis para el Release Gate (outer loop). Read-only; devuelve veredictos, no escribe estado.',
  phases: [
    { title: 'Reviewers', detail: '5 reviewers pesados en paralelo sobre el diff acumulado' },
    { title: 'Integration', detail: 'journey completo con deps reales (SECUENCIAL, fuera del parallel)' },
  ],
}

const RELEASE_REVIEW_SCHEMA = {
  type: 'object', additionalProperties: false,
  required: ['pass', 'findings'],
  properties: {
    pass: { type: 'boolean', description: 'true solo si no hay hallazgos bloqueantes y se pudo verificar' },
    findings: { type: 'array', items: { type: 'string' } },
  },
}

// diffRange y si la release tiene UI llegan por `args` (la skill los computa READ-ONLY).
const diffRange = (args && args.diffRange) || '<merge-anterior>..main'
const hasUI = !!(args && args.hasUI)

// --- PASO A · Reviewers pesados EN PARALELO (exactamente estos 5) ------------
// Cada uno delega en su subagente sobre el MISMO diff acumulado y devuelve síntesis.
// Barrera deliberada: la síntesis de release necesita los 5 veredictos juntos.
phase('Reviewers')
const REVIEWERS = [
  { gate: 'security',   agentType: 'security-reviewer' },
  { gate: 'smell',      agentType: 'simple-design-reviewer' },
  { gate: 'ux',         agentType: 'ux-krug-reviewer' },   // null SOLO si la release no tiene UI
  { gate: 'coherence',  agentType: 'coherence-three-way' },
  { gate: 'stack_arch', agentType: 'stack-guardian' },
]
const reviews = await parallel(REVIEWERS.map((r) => async () => {
  // N/A legítimo: ux sin UI → null (NO es fallo). El resto SIEMPRE corre.
  if (r.gate === 'ux' && !hasUI) return { gate: r.gate, value: null, na: true }
  try {
    const v = await agent(
      `Eres el reviewer pesado del Release Gate para el gate "${r.gate}". Revisa READ-ONLY el diff acumulado de la
release (${diffRange}) y devuelve tu veredicto + hallazgos bloqueantes. Si NO puedes verificar (herramienta
ausente, app no levantable), devuelve pass:false con el motivo — nunca PASS por defecto, nunca null por fallo.`,
      { label: `release:${r.gate}`, agentType: r.agentType, phase: 'Reviewers', schema: RELEASE_REVIEW_SCHEMA },
    )
    if (!v) return { gate: r.gate, value: false, error: 'sin veredicto' } // ausente/indeterminado = FALLO
    return { gate: r.gate, value: v.pass === true, findings: v.findings || [] }
  } catch (e) {
    return { gate: r.gate, value: false, error: 'el reviewer falló' }     // fallo = gate false, NUNCA null
  }
}))

// --- PASO B · integration SECUENCIAL, FUERA del parallel() ------------------
// integration NO va dentro del parallel() y NO delega en un reviewer: lo corre la skill
// verify/run con DEPS REALES (+ MCP chrome-devtools). Es el gate no negociable.
phase('Integration')
let integration
try {
  integration = await agent(
    `Recorre el JOURNEY COMPLETO de la release end-to-end con DEPENDENCIAS REALES (no stubs), usando la skill
verify/run (+ MCP chrome-devtools si hay UI). Diff: ${diffRange}. Devuelve si el journey camina entero. Si no
puedes levantarlo o falta una dep real → pass:false (nunca PASS por defecto).`,
    { label: 'release:integration', phase: 'Integration', schema: RELEASE_REVIEW_SCHEMA },
  )
} catch (e) {
  integration = { pass: false, findings: ['integration no pudo correr (deps reales/app no levantable)'] }
}

// --- PASO C · Síntesis → diagnóstico para releasing-a-version ----------------
// Mapea los 6 gates. Parciales NO promueven a passed: todos true (o ux=null por N/A) +
// integration=true → passed; cualquier otra cosa → failed.
const gates = {}
for (const rv of reviews.filter(Boolean)) gates[rv.gate] = rv.na ? null : rv.value
gates.integration = !!(integration && integration.pass === true)

const reviewersGreen = reviews.filter(Boolean).every((r) => r.na || r.value === true)
const status = (reviewersGreen && gates.integration === true) ? 'passed' : 'failed'

return {
  status,            // releasing-a-version lo persiste en releases[] (única escritora).
  gates,             // {security, smell, ux, coherence, stack_arch, integration}
  blocking: [
    ...reviews.filter((r) => r && r.value === false).map((r) => ({ gate: r.gate, error: r.error || null, findings: r.findings || [] })),
    ...(gates.integration === true ? [] : [{ gate: 'integration', findings: (integration && integration.findings) || [] }]),
  ],
  note: 'Read-only. releasing-a-version valida contra build-state.schema.json y escribe UNA entrada en releases[] (updated_by:releasing-a-version). Parciales NO promueven a passed.',
}
