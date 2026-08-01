# Priorización — Asistente de Empleabilidad IA

> **Framework**: Valor / Esfuerzo (matriz 2×2)
> **Fecha**: 2026-07-31
> **Participantes**: Propietario del producto (PO + dev)

## Contexto de la sesión

Se prioriza el backlog completo de 18 historias (todas en estado `lista`, aprobadas por INVEST + BDD) para decidir el orden de construcción del MVP. El producto es un pipeline con fuertes dependencias de secuencia (cimiento → negocio), por lo que la matriz Valor/Esfuerzo se usa para (a) confirmar qué es quick win vs. big bet y (b) validar qué es lo único diferible; el **orden de ejecución final respeta las dependencias técnicas** (Anexo A del PRD), no solo el cuadrante.

**Criterios operativos**:
- **Esfuerzo**: complejidad S/M → *bajo*; L → *alto*.
- **Valor**: impacto directo en el journey del MVP y en los objetivos O1–O5. El cimiento (identidad, tenancy) es *alto valor* porque habilita todo lo demás.

---

## Aplicación del framework — Valor / Esfuerzo

```
        Alto valor
            │
  QUICK WINS (bajo esf.)      │      BIG BETS (alto esf.)
  HU-001 HU-002 HU-003        │      HU-010 (buscar vacantes)
  HU-004 HU-005 HU-006        │      HU-013 (generar CV)
  HU-007 HU-009 HU-011        │      HU-016 (match ATS)
  HU-014 HU-017               │
 ───────────────────────────┼───────────────────────────
  FILL-INS (bajo esf.)        │      SKIP / APLAZAR (alto esf.)
  HU-012 (historial)          │      HU-008 (PDF vía LLM, v1.1)
  HU-015 (prompts BD)         │
  HU-018 (descarte)           │
            │
        Bajo valor
   Bajo ←──┼──→ Alto esfuerzo
```

### Cuadrantes

**Quick wins** (alto valor, bajo esfuerzo) — *hacer primero*:
HU-001, HU-002, HU-003, HU-004, HU-005, HU-006, HU-007, HU-009, HU-011, HU-014, HU-017

**Big bets** (alto valor, alto esfuerzo) — *planificar con spike, son núcleo del producto*:
HU-010 (buscar vacantes), HU-013 (generar CV), HU-016 (match ATS)

**Fill-ins / refactor** (menor valor, bajo esfuerzo) — *acompañan a su épica*:
HU-012 (historial de vacantes), HU-015 (gestor de prompts), HU-018 (sugerir descarte)

**Skip / aplazar** (menor valor relativo, alto esfuerzo) — *fuera del MVP*:
HU-008 (complementar perfil con PDF vía LLM) → diferida a **v1.1** (consistente con la línea de MVP del Story Map).

---

## Orden de ejecución recomendado (valor + dependencias)

El orden combina el valor de cada cuadrante con las dependencias duras del pipeline (cimiento antes que negocio; una big bet va con sus fill-ins de la misma épica):

| Orden | HU | Cuadrante | Épica | Razón de secuencia |
|---|---|---|---|---|
| 1 | HU-001 | Quick win | EP-001 | Puerta de entrada; sin login no hay nada |
| 2 | HU-002 | Quick win | EP-001 | Alta automática |
| 3 | HU-003 | Quick win | EP-001 | Sesión por User_ID → habilita tenancy |
| 4 | HU-004 | Quick win | EP-002 | Esquema multi-tenant (cimiento transversal) |
| 5 | HU-005 | Quick win | EP-002 | Filtrado por tenant |
| 6 | HU-006 | Quick win | EP-002 | Guarda + prueba de aislamiento (KPI O3) |
| 7 | HU-007 | Quick win | EP-003 | Perfil base desde LinkedIn |
| 8 | HU-009 | Quick win | EP-003 | Persistir/consultar perfil |
| 9 | HU-010 | **Big bet** | EP-004 | Motor de búsqueda (spike API/scraping, mock-first) |
| 10 | HU-011 | Quick win | EP-004 | Dedup por usuario |
| 11 | HU-012 | Fill-in | EP-004 | Historial de vacantes |
| 12 | HU-013 | **Big bet** | EP-005 | Generar CV (spike latencia LLM) |
| 13 | HU-014 | Quick win | EP-005 | Export PDF/DOCX (insumo del auditor) |
| 14 | HU-015 | Fill-in | EP-005 | Gestor de prompts en BD |
| 15 | HU-016 | **Big bet** | EP-006 | Match ATS (microservicio Python, mock-first) |
| 16 | HU-017 | Quick win | EP-006 | Enriquecimiento < 90% (regla O2) |
| 17 | HU-018 | Fill-in | EP-006 | Descarte + persistencia (cierra flujo) |
| — | HU-008 | Skip/aplazar | EP-003 | **v1.1** — no bloquea el MVP |

---

## Decisiones tomadas

1. **HU-008 es el único diferible**: se aplaza a v1.1 por ser alto esfuerzo (LLM local + parsing + fusión + conflictos) y valor incremental (el perfil ya se autopobla desde LinkedIn en HU-007). Consistente con la línea de MVP del Story Map.
2. **Las 3 big bets (HU-010, HU-013, HU-016) llevan spike previo**: validar disponibilidad del portal (API vs scraping), latencia del LLM local, e infraestructura del microservicio Python (mock-first) antes de comprometer estimación.
3. **El orden de ejecución respeta dependencias**, no solo el cuadrante: el cimiento (HU-001…006) va primero aunque sean quick wins, porque habilita todo; cada big bet va junto a los fill-ins de su épica para entregar la actividad del journey completa.
4. **Sin sobre-alcance**: 17 de 18 HU entran al MVP; el recorte (HU-008) es explícito y acordado, no una decisión implícita.

## Disidencias / preguntas abiertas

- Ninguna disidencia (sesión de un solo stakeholder).
- **Riesgo de sesión unipersonal**: al ser un solo stakeholder (PO+dev), el orden relativo de las 3 big bets no se sometió a tensión crítica. Recomendación: revisión informal de ~30 min con un segundo par (usuario final / tech lead) **antes de arrancar el spike de HU-010**, para validar orden y criterios de valor.
- **Pendiente de sprint planning**: fijar el valor de N (umbral de palabras clave) del Escenario 1 de HU-013 para que su test sea ejecutable sin ambigüedad.

## Próxima revisión

- Fecha tentativa: al cierre del primer slice de cimiento (EP-001), o antes de arrancar la primera big bet (HU-010). Re-priorizar si un spike cambia el esfuerzo estimado de una big bet.
