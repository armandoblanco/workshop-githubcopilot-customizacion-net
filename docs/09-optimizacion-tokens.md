# Módulo 9: Optimización de tokens y AI Credits

Tiempo estimado: 20 minutos. Este módulo es de análisis, no de código: vas a auditar el costo de todo lo que construiste en los módulos anteriores.

## Por qué este módulo existe

El 1 de junio de 2026 GitHub Copilot pasó a facturación por uso. Los premium requests (PRUs) desaparecieron para los planes mensuales y fueron reemplazados por **GitHub AI Credits**: cada crédito equivale a $0.01 USD y el consumo se calcula por **tokens** (entrada, salida y caché) a la tarifa API publicada de cada modelo. Los planes anuales de Pro/Pro+ siguen en el modelo legacy de premium requests hasta su renovación, con multiplicadores más altos desde junio.

Lo que sigue igual y lo que cambió:

| | Antes (PRUs) | Ahora (AI Credits) |
|---|---|---|
| Unidad de cobro | Interacción (request) | Tokens consumidos |
| Un prompt corto vs una sesión agéntica larga | Costaban lo mismo | Cuestan lo que consumen |
| Code completions y Next Edit Suggestions | Incluidos | Siguen incluidos, no consumen créditos |
| Chat, modo agente, CLI, cloud agent | 1+ requests por multiplicador | Tokens a tarifa del modelo |
| Copilot code review | Requests | Créditos **más minutos de GitHub Actions** |
| Al agotar la cuota | Fallback a modelo incluido | Se detiene (o paga overage si hay presupuesto habilitado) |

La consecuencia para este workshop: **todo lo que construiste en los módulos 2 a 8 tiene ahora un precio por turno**. Las instrucciones viajan en cada mensaje, las skills cargan contexto al activarse, los agentes eligen modelos con tarifas distintas. Personalizar bien y optimizar costos se volvieron el mismo problema.

Advertencia de vigencia: las cifras de créditos incluidos por plan, los bonos flex del periodo de transición y las tarifas por modelo cambian. No las memorices de este documento; verifícalas en la [página de planes](https://github.com/features/copilot/plans) y en la [documentación de billing](https://docs.github.com/en/copilot/concepts/billing-and-payments). Lo que este módulo enseña es el modelo mental, que es estable.

## El modelo mental: el efecto re-envío

Lo único que necesitas entender para razonar sobre costos es esto: **el modelo no tiene memoria**. En cada turno del chat, Copilot re-envía el contexto completo: system prompt, instrucciones del repo, el agente activo, las skills cargadas, el historial de la conversación y los archivos adjuntos. Tu pregunta de una línea en el turno 30 de una sesión no cuesta una línea: cuesta la sesión entera como entrada, otra vez.

Tres consecuencias directas:

1. **Las sesiones largas crecen en costo por turno.** El turno 30 es mucho más caro que el turno 3, aunque preguntes lo mismo.
2. **En trabajo agéntico, la entrada domina.** Un agente que itera (lee archivos, ejecuta, corrige, repite) re-envía el contexto en cada vuelta interna. La salida (el código generado) suele ser la fracción menor de la cuenta.
3. **Todo lo que se carga "siempre" se paga siempre.** Un `copilot-instructions.md` de 200 líneas viaja en cada turno de cada sesión de cada desarrollador del repo.

El caché de tokens amortigua esto (los tokens cacheados cobran tarifa reducida), pero la reduce, no la elimina, y no todo el contexto es cacheable entre turnos.

## Auditoría de lo que construiste, capa por capa

Repasa el repo del workshop con lentes de costo:

**Instrucciones (módulo 2).** Es la capa con peor perfil de costo por diseño: carga incondicional en cada turno. La disciplina: `copilot-instructions.md` corto y las reglas específicas en archivos `*.instructions.md` con `applyTo`, que solo suman contexto cuando el archivo en cuestión está en juego. Esto ya lo hiciste por razones de calidad en el módulo 2; ahora también es la decisión barata.

**Prompt files (módulo 3).** Perfil sano: costo cero hasta que invocas con `/`, y su contenido se paga una vez por invocación. Sin cambios.

**Custom agents (módulo 4).** Aquí está la palanca más grande: el campo `model`. La diferencia de tarifa entre un modelo frontier y uno estándar es de un orden de magnitud o más en tokens de salida. El patrón del módulo 4 (Opus para el arquitecto y el auditor que razonan, Sonnet para el implementador que ejecuta) es exactamente el patrón de costo correcto: **modelo caro para decisiones, modelo barato para volumen**. Si todo tu equipo usa el modelo más caro para renombrar variables, ningún otro consejo de este módulo compensa eso.

**Subagents (módulo 4.1).** Doble filo. A favor: cada subagent tiene contexto aislado, así que el coordinador no arrastra los detalles de cada worker en su propio historial, y eso corta el efecto re-envío. En contra: la revisión en paralelo con tres workers son tres sesiones consumiendo a la vez. El paralelo multi-perspectiva es para código que lo amerita (el módulo de cálculo financiero), no para cada PR.

**Skills (módulos 5 y 5.1).** La carga en tres niveles es una arquitectura de ahorro: discovery casi gratis, cuerpo al activarse, recursos a demanda. La skill `reportes-regulatorios` del módulo 5.1, con varias plantillas en `references/` de las que solo se carga la que aplica, es el patrón correcto; una skill que mete las tres plantillas (400+ líneas) en el SKILL.md paga esas líneas en cada activación. Revisa también los flags: una skill con `description` vaga que se auto-carga en conversaciones donde no aporta es contexto desperdiciado en silencio.

**Hooks (módulo 8).** No consumen tokens: son shell, no modelo. Un `preToolUse` que corta un comando inútil temprano ahorra las vueltas de agente que ese error habría causado. Es la única capa del workshop con costo de tokens estrictamente cero.

## Hábitos de sesión que mueven la aguja

Más impacto que cualquier configuración:

1. **Sesión nueva por tarea.** El chat eterno de todo el día es el error de costo número uno. Cerrar y abrir chat pone el contador de contexto en cero. Si necesitas arrastrar conclusiones, pide un resumen de tres líneas y pégalo en la sesión nueva.
2. **Contexto explícito y mínimo.** Adjuntar el archivo relevante es más barato y más preciso que dejar que el agente explore el repo a ciegas durante seis vueltas de tool calls.
3. **Completions para lo mecánico.** Las code completions y Next Edit Suggestions no consumen créditos. Boilerplate, renombres y ajustes pequeños con completions; el chat y el agente para lo que de verdad requiere razonamiento.
4. **Corta las iteraciones fallidas.** Si el agente lleva tres vueltas sin converger, cada vuelta adicional re-paga todo el contexto acumulado. Detén, replantea el prompt con más precisión o cambia de enfoque. Insistirle "inténtalo de nuevo" al mismo contexto es la forma más cara de no avanzar.
5. **Mide antes de optimizar.** El dashboard de billing (usuario y organización) muestra el consumo por feature y modelo. Una semana de datos reales de tu equipo vale más que cualquier lista de tips, incluida esta.

## Controles administrativos

La optimización individual no sustituye la gobernanza:

- **Presupuestos.** Configura budgets sobre el uso medido; sin presupuesto habilitado el trabajo se detiene al agotar los créditos incluidos, y con la política de pago habilitada el gasto puede crecer sin techo si nadie lo fija. Un budget de $0 hace imposible la sorpresa en la factura.
- **Pooling organizacional.** En Business y Enterprise los créditos incluidos se agrupan a nivel organización: los usuarios ligeros subsidian a los pesados. Bien para flexibilidad, pero hace más importante la visibilidad por usuario para detectar consumo anómalo.
- **Code review cuenta doble.** Desde junio de 2026 consume créditos y minutos de Actions. Si activaste review automático en cada PR (módulo 6), ese costo es recurrente y silencioso; revisa si lo necesitas en todos los repos o solo en los críticos.

## La parte crítica: qué es teatro y qué no

Circula mucho consejo de "ahorro de tokens" que no resiste una medición. Separemos:

**Funciona, con matices: comprimir la salida (caveman).** La skill [caveman](https://github.com/JuliusBrussee/caveman) que instalaste en el módulo 5.1 reduce de forma medida (~65% según el propio proyecto) los tokens de **salida** al eliminar relleno conversacional. El matiz importante: en sesiones agénticas el costo suele estar dominado por la **entrada** re-enviada, que caveman no toca. Es un ahorro real en chat conversacional con respuestas largas; es marginal en un agente que itera sobre un repo. Detalle interesante del propio proyecto: midieron que trucos como reemplazar palabras por flechas ahorran cero tokens con los tokenizadores reales. Hasta la skill de comprimir tokens tuvo que aprender a no hacer teatro.

**Funciona: contexto en Markdown (markitdown).** Convertir un PDF o DOCX a Markdown con [MarkItDown](https://github.com/microsoft/markitdown) antes de dárselo al agente reduce el volumen de tokens frente a formatos crudos y mejora lo que el modelo entiende. Además la conversión es un script: costo de modelo cero.

**Teatro: micro-optimizar prompts mientras el contexto base está inflado.** Recortar diez palabras de tu pregunta mientras arrastras un `copilot-instructions.md` de 300 líneas y una sesión de 40 turnos es optimizar el 0.1% del problema. El orden de ataque es: sesiones, luego contexto siempre-cargado, luego modelo por tarea, y al final, si queda apetito, el estilo del prompt.

**Teatro: abreviar dentro del código.** Nombres de variables cortos "para ahorrar tokens" degradan el código que humanos mantienen, y el ahorro es irrelevante contra el costo del contexto re-enviado. El código se escribe para las personas; los ahorros se buscan en la sesión.

**Depende: evitar el modo agente.** Hay quien recomienda volver a chat simple para ahorrar. A veces sí: para una pregunta puntual, el modo agente que explora el repo es un desperdicio. Pero un agente que resuelve en una sesión lo que a ti te tomaría cinco sesiones de chat con ida y vuelta puede ser más barato en total. La métrica correcta es costo por tarea terminada, no costo por turno.

## Ejercicio de cierre

Con el billing dashboard abierto (o el de un compañero que lleve días usando Copilot):

1. Identifica qué feature domina el consumo (chat, agente, code review, CLI).
2. Identifica el modelo que domina el consumo y pregúntate si las tareas que lo usan lo justifican.
3. Propón un cambio de configuración de este workshop que atacaría ese consumo (un `model` distinto en un agente, mover reglas a `applyTo`, apagar review automático en repos no críticos) y estima el efecto.

Si no hay datos disponibles, haz el ejercicio en seco con la sesión más larga de este workshop: cuenta los turnos y razona cuánto contexto viajó en el último.

## Referencias

- [Anuncio oficial de la transición a facturación por uso](https://github.blog/news-insights/company-news/github-copilot-is-moving-to-usage-based-billing/) (GitHub Blog, abril de 2026).
- [Discusión oficial con FAQ de la transición](https://github.com/orgs/community/discussions/192948).
- [Planes y precios de Copilot](https://github.com/features/copilot/plans) y [documentación de billing](https://docs.github.com/en/copilot/concepts/billing-and-payments) para cifras vigentes.
- [Requests legacy (planes anuales)](https://docs.github.com/en/copilot/reference/copilot-billing/request-based-billing-legacy/copilot-requests) si tu organización aún tiene usuarios en el modelo anterior.

## Cierre

La versión honesta de este módulo en tres líneas: el costo vive en el contexto que se re-envía, no en lo que escribes; la palanca más grande es qué modelo hace qué tarea; y las cuatro capas del workshop, bien usadas, ya son la arquitectura de ahorro. Optimizar tokens no es una disciplina aparte de personalizar Copilot: es personalizarlo bien.

## Siguiente

Vuelve al [Módulo 7: Cuándo usar qué](07-cuando-usar-que.md) para el árbol de decisión completo, ahora con hooks y costos en mente, o al [índice del workshop](../README.md).
