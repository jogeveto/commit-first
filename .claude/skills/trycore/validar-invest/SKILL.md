---
name: trycore-validar-invest
description: Valida una o varias Historias de Usuario contra los 6 criterios INVEST (Independent, Negotiable, Valuable, Estimable, Small, Testable). Reporta ✓/✗ por letra con explicación y propuesta de fix. Internamente puede invocar al agente `invest-validator` para segunda opinión. Path-scope principal: lee `docs/04-historias/HU-*.md`. Quality gate transversal antes de marcar `estado: lista`.
category: Quality
tags: [invest, validacion, calidad, user-story, trycore]
---

# trycore-validar-invest

Valida una historia (o un conjunto) contra los 6 criterios INVEST y reporta resultado detallado.

## Contexto (fuente: METODOLOGIA.md §3)

| Letra | Test rápido |
|---|---|
| **I** — Independent | ¿Puedo planearla en cualquier sprint sin esperar a otra? |
| **N** — Negotiable | ¿Hay espacio para que el equipo discuta el "cómo"? El "qué" es fijo, el "cómo" no. (3 Cs de Ron Jeffries: Card, Conversation, Confirmation) |
| **V** — Valuable | ¿Si la cuento al cliente, le importa? Valor externo y visible, no interno técnico. |
| **E** — Estimable | ¿El equipo sabe cuánto cuesta? Bill Wake (2025): si se replanteara, "E" sería "External". |
| **S** — Small | ¿Cabe en 1 sprint, idealmente 2-4 días? Si es más, dividir en **slices verticales**. |
| **T** — Testable | ¿Tiene AC claros y verificables? Sin AC, la "T" falla por defecto. |

## Inputs requeridos

- Path(s) a las historias a validar. Si se pasa un directorio, validar todas las historias dentro.
- (Opcional) `--strict` para aplicar la reflexión de Bill Wake (la "E" se reinterpreta como "External value").

## Reglas duras

1. **Toda historia debe pasar las 6 letras** antes de marcarse `estado: lista` en su frontmatter.
2. **Si T falla** porque la historia no tiene AC → no es válido decir "T pendiente"; sugerir `/trycore:ac` primero.
3. **Si S falla** → proponer descomposición en slices verticales (no horizontales tipo "primero front, luego back").
4. **Si V es ambiguo** → preguntar al usuario quién es el beneficiario y qué valor recibe. Si no se puede responder, la historia no debería existir.

## Flujo de la skill

1. **Listar historias** a validar (o tomar la que el usuario apuntó).
2. **Para cada historia**, evaluar las 6 letras con razonamiento explícito.
3. **Generar reporte inline** (tabla 6×N) por historia:

   | Historia | I | N | V | E | S | T | Fix sugerido |
   |---|---|---|---|---|---|---|---|
   | HU-001 | ✓ | ✓ | ✓ | ✓ | ✗ | ✓ | Dividir en HU-001a y HU-001b por capacidad |

4. **Si el contexto lo amerita** (>5 historias, o ambigüedad técnica), invocar al agente `invest-validator` para segunda opinión.
5. **Sugerir** al usuario actualizar el frontmatter `estado: lista` solo en las historias que pasaron las 6.

## Handoff

- Si todas pasan: sugerir avanzar al backlog (`/trycore:backlog`) o priorización (`/trycore:priorizar`).
- Si fallan: sugerir corregir y re-validar.

## Anti-pattern: "INVEST pase libre"

Marcar todas las cajas como ✓ sin razonamiento es peor que no validar. La skill debe **siempre** mostrar el razonamiento detrás de cada ✓ y especialmente detrás de cada ✗. Si no hay fundamento, marcar como "?" y preguntar al usuario.
