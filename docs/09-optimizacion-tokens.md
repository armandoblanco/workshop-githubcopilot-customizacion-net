# Módulo 9: Optimización de tokens y AI Credits

Tiempo estimado: 20-25 minutos. Este módulo es de análisis, no de código: vas a auditar el costo de todo lo que construiste en los módulos anteriores y a llevarte las lecciones más importantes para operar barato bajo AI Credits.

> Este módulo condensa el [handbook de optimización de tokens de GitHub Copilot](https://github.com/armandoblanco/github-copilot-token-optimization) (Armando Blanco, MIT) y los decks de token economics de Paula Silva ([handbook](https://agenticdevopsplatform.ai/decks/GitHubCopilotTokenOptimizationHandbook_Deck_v3_0_1_2026-06-15_multi.html) y [workshop](https://agenticdevopsplatform.ai/decks/GitHubCopilotTokenOptimizationWorkshop_Deck_v2_0_0_2026-05-29_multi.html)), adaptados al escenario de Contoso Banco.

## Por qué este módulo existe

El 1 de junio de 2026 GitHub Copilot pasó a facturación por uso. Los premium requests (PRUs) desaparecieron para los planes mensuales y fueron reemplazados por **GitHub AI Credits**: cada crédito equivale a $0.01 USD y el consumo se calcula por **tokens** (entrada, salida y caché) a la tarifa API publicada de cada modelo. Los planes anuales de Pro/Pro+ siguen en el modelo legacy de premium requests hasta su renovación, con multiplicadores más altos desde junio.

Lo que sigue igual y lo que cambió:

| | Antes (PRUs) | Ahora (AI Credits) |
|---|---|---|
| Unidad de cobro | Interacción (request) | Tokens consumidos (entrada + salida + caché) |
| Un prompt corto vs una sesión agéntica larga | Costaban lo mismo | Cuestan lo que consumen |
| Code completions y Next Edit Suggestions | Incluidos | Siguen incluidos, no consumen créditos |
| Chat, modo agente, CLI, cloud agent | 1+ requests por multiplicador | Tokens a tarifa del modelo |
| Copilot code review | Requests | Créditos **más minutos de GitHub Actions** |
| Al agotar la cuota | Fallback a modelo incluido | Se detiene (o paga overage si hay presupuesto habilitado) |

**Lo que sigue gratis:** las completions inline (ghost text) y Next Edit Suggestions no consumen créditos en ningún plan de pago. Si tu flujo se centra en autocompletar mientras escribes, tu factura no cambia. **Lo que ahora tiene costo medido:** Chat (ask y agent), Copilot CLI, Cloud Agent y Code Review.

**Dos modelos están incluidos sin costo adicional** en los planes de pago: **GPT-4.1** y **GPT-5 mini**. Los otros 20+ modelos del catálogo consumen créditos a su tarifa. La diferencia es dramática: una sesión típica de Chat (10K de entrada, 1K de salida) cuesta ~$0.005 con GPT-5 mini y ~$0.045 con un modelo frontier como Claude Sonnet — unas **9 veces más**.

La consecuencia para este workshop: **todo lo que construiste en los módulos 2 a 8 tiene ahora un precio por turno**. Las instrucciones viajan en cada mensaje, las skills cargan contexto al activarse, los agentes eligen modelos con tarifas distintas. Personalizar bien y optimizar costos se volvieron el mismo problema.

Advertencia de vigencia: los créditos incluidos por plan, los nombres/versiones de modelos y sus tarifas cambian. No los memorices de este documento; verifícalos en la [página de planes](https://github.com/features/copilot/plans), en [Models and pricing](https://docs.github.com/en/copilot/reference/copilot-billing/models-and-pricing) y en la [documentación de billing](https://docs.github.com/en/copilot/concepts/billing-and-payments). Lo que este módulo enseña es el modelo mental, que es estable.

## La lección número uno: el efecto re-envío

Si te llevas una sola cosa de este módulo, que sea esta: **el modelo no tiene memoria**. En cada turno, Copilot re-envía el contexto completo al modelo: system prompt, definiciones de tools, instrucciones del repo, el agente activo, las skills cargadas, los resultados de tool calls anteriores, el historial completo de la conversación y tu prompt actual. Tu pregunta de una línea en el turno 30 no cuesta una línea: cuesta la sesión entera como entrada, otra vez.

Los números lo hacen concreto: una sesión de 50K tokens que dura 40 turnos envía **~2 millones de tokens de entrada acumulados**, aunque tu último prompt haya sido de 20 palabras. La **inflación de contexto** (context bloat) es la fuente número uno de tokens desperdiciados.

Tres consecuencias directas:

1. **Las sesiones largas crecen en costo por turno.** El turno 30 es mucho más caro que el turno 3, aunque preguntes lo mismo.
2. **En trabajo agéntico, la entrada domina.** Un agente que itera (lee archivos, ejecuta, corrige, repite) re-envía el contexto en cada vuelta interna. La salida suele ser la fracción menor de la cuenta de entrada — pero ojo: por token, la **salida cuesta 4x a 6x más** que la entrada, así que un diff de 30K tokens pesa mucho.
3. **Todo lo que se carga "siempre" se paga siempre.** Un `copilot-instructions.md` de 200 líneas viaja en cada turno de cada sesión de cada desarrollador del repo. Es un impuesto recurrente.

El caché de tokens amortigua esto (los tokens cacheados cobran tarifa reducida), pero la reduce, no la elimina, y no todo el contexto es cacheable entre turnos. Ventanas de contexto más grandes **no** significan facturas más baratas: los tokens se cobran linealmente uses el 20% o el 80% de la ventana.

## Cómo se construye el contexto en cada superficie

Entender qué entra en el paquete de contexto es el prerequisito para optimizar cualquier cosa.

- **Completions inline (gratis).** Ventana reducida (~8-16K tokens), construida localmente en el IDE con el código antes del cursor (prefix), después (suffix) vía Fill-in-the-Middle, y fragmentos de los tabs abiertos por similitud textual. No indexa el repo completo. Como son gratis, aquí optimizas por **relevancia**, no por costo: menos ruido en los tabs = mejores sugerencias.
- **Chat y Agent mode.** Ventana grande (128K a ~1M según el modelo). Incluye el archivo activo, la selección, lo referenciado con `@`, el repo indexado, `copilot-instructions.md` y el historial completo. Copilot reserva ~30% de la ventana para la respuesta; no es configurable.
- **Agent mode y CLI: los mayores consumidores.** El agente lee archivos **completos** (no fragmentos), ejecuta búsquedas con resultados verbosos, acumula output de terminal (build logs, tests) y mantiene toda la sesión en contexto. Una sola tarea agéntica puede consumir el equivalente a 10-20 premium requests del modelo anterior.
- **Compaction.** Cuando la ventana llega a ~80-95%, Copilot resume el historial automáticamente. Esto consume tokens extra y puede degradar la calidad porque el modelo pierde detalle. Gestionar el contexto tú mismo, antes de que se dispare, es más barato y más preciso.

Dónde se queman tokens típicamente en sesiones agénticas: leer archivos completos cuando solo importa una función, sesiones largas sin limpiar, `copilot-instructions.md` sobredimensionado, output verboso de herramientas pipeado al contexto, y usar un modelo de razonamiento pesado para ediciones rutinarias.

## Las cinco palancas de eficiencia (ordenadas por impacto)

Ninguna táctica es dramática por sí sola, pero **se apilan multiplicativamente**: combinadas rutinariamente cortan el 60-70% del gasto en tokens sin cambiar lo que entregas. La higiene de contexto da las ganancias más grandes; la medición cierra el ciclo.

### Palanca 1 — Higiene de contexto (la de mayor impacto)

No esperes a que Copilot compacte solo. Gestiona el contexto proactivamente. En Copilot CLI, estos comandos se pagan solos:

| Comando | Qué hace | Cuándo usarlo |
|---|---|---|
| `/clear` o `/new` | Resetea la conversación por completo | Entre tareas no relacionadas |
| `/compact` | Resume la sesión y libera la mayor parte de la ventana | A mitad de tarea, antes de cambiar de foco |
| `/context` | Desglose del uso actual de tokens por categoría | Antes de `/compact`, para decidir si vale la pena |
| `/usage` | Totales de tokens por modelo, duración, archivos tocados | Al final de cada tarea, para saber cuánto costó |
| `/resume` | Reabre una sesión previa con su resumen guardado | Al retomar una tarea sin empezar de cero |

Regla mental: **piensa las sesiones como branches**, una nueva por tarea. Corre `/context` antes de `/compact` (si estás por debajo del ~40% no necesitas compactar). Compacta antes de cambiar de foco, no después de que la ventana esté llena.

### Palanca 2 — Disciplina de prompts

Prompts más cortos y enfocados no solo ahorran entrada; reducen el volumen de salida y la cantidad de tool calls. Compara:

- **Caro:** "Mira mi repo y averigua por qué falla el login. Revisa auth, base de datos, middleware y logs. Arréglalo." → escanea muchos archivos y quema tokens antes de acotar.
- **Eficiente:** `/plan` + "En `@src/auth/login.ts`, `handleLogin` retorna 500 cuando el email tiene unicode. Propón un fix." → un archivo, una función, un caso.

Tácticas validadas: usa **Plan mode** antes de codificar cualquier cosa que no sea un cambio de una línea; referencia archivos con `@ruta/archivo` en vez de directorios completos; **una tarea por prompt**. Cinco preguntas enfocadas en una sesión cuestan menos que cinco sesiones de una pregunta (cada sesión nueva paga overhead de inicialización). Es el principio de Anthropic: el conjunto más pequeño posible de tokens de alta señal.

### Palanca 3 — Selección de modelo

Bajo billing por tokens, los modelos pequeños son dramáticamente más baratos. Referencia ilustrativa de costo por sesión típica (10K entrada, 1K salida; verifica cifras vigentes):

| Modelo | Costo aprox. | Tier |
|---|---|---|
| GPT-5 mini / GPT-4.1 | ~$0.005 | Incluido |
| Claude Sonnet | ~$0.045 | Premium |
| GPT-5 | ~$0.05 | Premium |
| Claude Opus | ~$0.075 | Premium |

El patrón correcto: **modelo caro para decisiones, modelo barato para volumen**. En CLI, cambia con `/model` a mitad de sesión: planifica con un modelo de razonamiento, implementa con uno intermedio, revisa con uno económico. Es exactamente lo que hiciste en el módulo 4 (Opus para arquitecto/auditor, Sonnet para implementador). Y como la salida cuesta 4x-6x la entrada, si optimizas para créditos, optimiza para longitud de salida.

### Palanca 4 — Alcance y control de herramientas

Limitar lo que Copilot puede ver es la forma más confiable de que no queme tokens en lo que no importa.

- **Content Exclusion (admin).** La palanca más poderosa a nivel infraestructura. Un archivo/directorio excluido se ignora en completions, contexto, Chat (ask) y Code Review. Se configura por enterprise, org o repo. Aviso clave: **Copilot CLI, Cloud Agent y Agent mode NO respetan Content Exclusion** (corren en entornos efímeros). Para el agente, mitiga con reglas en `AGENTS.md`, hooks `preToolUse` que bloqueen paths sensibles (módulo 8), o restringiendo Agent mode por política.
- **Config a nivel de repo** (`.vscode/settings.json`, commiteable): desactiva Copilot para tipos de archivo de bajo valor y excluye carpetas de build de la indexación:

```json
{
  "github.copilot.enable": {
    "*": true, "yaml": false, "json": false, "plaintext": false, "properties": false
  },
  "files.exclude": { "**/bin": true, "**/obj": true },
  "search.exclude": { "**/bin": true, "**/obj": true }
}
```

- **`copilot-instructions.md` mínimo.** Se inyecta en cada request de Chat: cada línea es impuesto recurrente. Mantenlo **bajo ~20 líneas**, solo estándares que el modelo no puede inferir del código y reglas de "no hacer", **sin links a otros `.md`** (los links se cargan como contexto adicional y generan inflación involuntaria). El detalle por subsistema va en `*.instructions.md` con `applyTo` (módulo 2), que solo suma contexto cuando el path aplica.
- **En CLI:** `/cwd` y `/add-dir` acotan el directorio visible; `--allow-tool`/`--deny-tool` detienen comandos que inflarían el output.

### Palanca 5 — Medición

No puedes optimizar lo que no mides. Copilot CLI expone `/context` (uso en tiempo real), `/usage` (contabilidad por sesión: tokens por modelo, duración, archivos) y **OTel traces** que puedes pipear a Azure Monitor o Grafana para ver el desperdicio a nivel de equipo. El **billing dashboard de AI Credits** en GitHub.com muestra consumo por modelo y periodo, y permite alertas al 75%, 90% y 100% del presupuesto. Establece un baseline corriendo `/usage` las primeras semanas.

## Subagentes que ahorran tokens

Los comandos especializados de Copilot CLI corren con su **propio contexto acotado** y devuelven solo el resumen a la sesión principal. Un subagente puede leer cinco archivos para responder, pero tu sesión principal solo recibe la respuesta, no los cinco archivos:

| Comando | Uso | Por qué ahorra |
|---|---|---|
| `/explore` | Q&A rápido sobre el codebase | Las lecturas no contaminan la conversación principal |
| `/plan` | Plan de implementación antes de codificar | Barato y de alto impacto: un prompt guía el resto |
| `/task` | Ejecuta tests y builds | Reporta breve en éxito, output completo solo en fallo |
| `/review` | Code review de alta señal | Surfacea problemas reales, salta nits |
| `/delegate` | Entrega al cloud agent (async, retorna PR) | No es facturable contra tu sesión local |

Es el mismo patrón de las skills con `references/` (módulo 5.1) y de los subagents del módulo 4.1: aislar contexto para no arrastrarlo en la ventana principal.

## Prácticas de IDE (sin permisos de admin)

- **Máximo 3-5 tabs abiertos.** Copilot escanea los tabs para completions inline; usa "Close Other Editors" al cambiar de tarea. En monorepos, abre solo la carpeta del servicio actual.
- **Un servicio a la vez.** Archivos de varios servicios abiertos expanden el contexto sin mejorar las sugerencias.
- **Prefiere completions inline sobre Chat** para código rutinario: son gratis y suelen ser la opción correcta en calidad y costo.
- **Usa Snooze** (Status Bar de VS Code) al leer código, debuggear o revisar PRs, cuando no quieres sugerencias.
- **Sesiones cortas en Agent mode** con criterios de éxito claros: "refactoriza este método para usar Strategy" rinde más que "mejora toda esta clase".

## Auditoría de lo que construiste, capa por capa

Repasa el repo del workshop con lentes de costo:

**Instrucciones (módulo 2).** Peor perfil de costo por diseño: carga incondicional en cada turno. La disciplina: `copilot-instructions.md` corto (Palanca 4) y las reglas específicas en `*.instructions.md` con `applyTo`, que solo suman contexto cuando el archivo en juego coincide con el path.

**Prompt files (módulo 3).** Perfil sano: costo cero hasta que invocas con `/`, y su contenido se paga una vez por invocación. Sin cambios.

**Custom agents (módulo 4).** La palanca más grande está aquí: el campo `model` (Palanca 3). Modelo caro para decisiones, barato para volumen. Si todo tu equipo usa el modelo más caro para renombrar variables, ningún otro consejo compensa eso.

**Subagents (módulo 4.1).** Doble filo. A favor: contexto aislado que corta el re-envío. En contra: tres workers en paralelo son tres sesiones consumiendo a la vez. El paralelo multi-perspectiva es para código que lo amerita, no para cada PR.

**Skills (módulos 5 y 5.1).** La carga en tres niveles es una arquitectura de ahorro: discovery casi gratis, cuerpo al activarse, recursos a demanda. La skill `reportes-regulatorios` del módulo 5.1, con varias plantillas en `references/` de las que solo se carga la que aplica, es el patrón correcto; una skill que mete las tres plantillas (400+ líneas) en el SKILL.md paga esas líneas en cada activación. Revisa también los flags: una skill con `description` vaga que se auto-carga donde no aporta es contexto desperdiciado en silencio.

**Hooks (módulo 8).** No consumen tokens: son shell, no modelo. Un `preToolUse` que corta un comando inútil temprano ahorra las vueltas de agente que ese error habría causado. Es la única capa del workshop con costo de tokens estrictamente cero — y además tu mejor mitigación de Content Exclusion en Agent mode.

## Controles administrativos

La optimización individual no sustituye la gobernanza:

- **Presupuestos.** Configura budgets sobre el uso medido con alertas al 75/90/100%. Sin presupuesto habilitado el trabajo se detiene al agotar los créditos incluidos; con pago habilitado el gasto puede crecer sin techo si nadie lo fija.
- **Modelo default económico.** GPT-4.1 o GPT-5 mini como default para completions y chat rutinario; premium restringido por política a los equipos que lo necesitan.
- **Pooling organizacional.** En Business y Enterprise los créditos se agrupan a nivel org: los usuarios ligeros subsidian a los pesados. Bien para flexibilidad, pero hace más importante la visibilidad por usuario para detectar consumo anómalo.
- **Code review cuenta doble.** Desde junio de 2026 consume créditos y minutos de Actions. Si activaste review automático en cada PR (módulo 6), revisa si lo necesitas en todos los repos o solo en los críticos.

## La parte crítica: qué es teatro y qué no

Circula mucho consejo de "ahorro de tokens" que no resiste una medición. Separemos:

**Funciona, con matices: comprimir la salida (caveman).** La skill [caveman](https://github.com/JuliusBrussee/caveman) que instalaste en el módulo 5.1 reduce de forma medida (~65% según el propio proyecto) los tokens de **salida** al eliminar relleno conversacional. El matiz importante: en sesiones agénticas el costo suele estar dominado por la **entrada** re-enviada, que caveman no toca. Es un ahorro real en chat conversacional con respuestas largas; es marginal en un agente que itera sobre un repo. Detalle interesante del propio proyecto: midieron que trucos como reemplazar palabras por flechas ahorran cero tokens con los tokenizadores reales. Hasta la skill de comprimir tokens tuvo que aprender a no hacer teatro.

**Funciona: contexto en Markdown (markitdown).** Convertir un PDF o DOCX a Markdown con [MarkItDown](https://github.com/microsoft/markitdown) antes de dárselo al agente reduce el volumen de tokens frente a formatos crudos y mejora lo que el modelo entiende. Además la conversión es un script: costo de modelo cero.

**Teatro: micro-optimizar prompts mientras el contexto base está inflado.** Recortar diez palabras de tu pregunta mientras arrastras un `copilot-instructions.md` de 300 líneas y una sesión de 40 turnos es optimizar el 0.1% del problema. El orden de ataque es: sesiones, luego contexto siempre-cargado, luego modelo por tarea, y al final, si queda apetito, el estilo del prompt.

**Teatro: abreviar dentro del código.** Nombres de variables cortos "para ahorrar tokens" degradan el código que humanos mantienen, y el ahorro es irrelevante contra el costo del contexto re-enviado. El código se escribe para las personas; los ahorros se buscan en la sesión.

**Depende: evitar el modo agente.** Hay quien recomienda volver a chat simple para ahorrar. A veces sí: para una pregunta puntual, el modo agente que explora el repo es un desperdicio. Pero un agente que resuelve en una sesión lo que a ti te tomaría cinco sesiones de chat con ida y vuelta puede ser más barato en total. La métrica correcta es costo por tarea terminada, no costo por turno.

## Por qué funciona: context rot y attention budget

La optimización de contexto no es solo de costo; también es de **calidad**. Anthropic lo documentó en "Effective context engineering for AI agents":

- **Context rot.** A más tokens en la ventana, peor recuerda el modelo la información con precisión. Es una propiedad de la arquitectura transformer: cada token atiende a todos los demás, y a mayor contexto la atención se diluye.
- **Attention budget.** El modelo tiene un presupuesto de atención finito. Contexto irrelevante no solo cuesta créditos: **degrada activamente** la calidad al competir por atención con los tokens que sí importan.
- **Tool bloat.** Conjuntos de herramientas y archivos abiertos de más inflan el contexto sin aportar. No abras más archivos, repos o tools de los necesarios.

Por eso las cinco palancas se apilan: reducir contexto baja la factura **y** sube la calidad de las respuestas. La eficiencia de contexto no es un trade-off contra la productividad; es una precondición para la productividad sostenible.

## Checklist de optimización

**Repositorio (commitear, aplica al clonar):**
- `.gitignore` completo (build, generados, logs, archivos de IDE).
- `.vscode/settings.json` con Copilot desactivado para YAML/JSON/plaintext/properties y carpetas de build excluidas.
- `copilot-instructions.md` mínimo (< 20 líneas, sin links a otros `.md`); detalle por path en `*.instructions.md`.
- `AGENTS.md` con restricciones de paths si el equipo usa Agent mode.

**Hábitos de sesión (CLI/chat):**
- Sesión nueva por tarea (`/clear` o `/new`).
- `/plan` antes de codificar cualquier cambio no trivial.
- Referencias con `@ruta/archivo`, no directorios completos.
- `/context` → `/compact` proactivo antes de cambiar de foco.
- `/model` para bajar de tier cuando termine la parte difícil.
- `/usage` al cerrar cada tarea.

**Hábitos de IDE (diarios):**
- Máximo 3-5 tabs; un servicio a la vez.
- Completions inline (gratis) para lo rutinario; Chat para lo que requiere razonamiento.
- Snooze al leer/debuggear/revisar PRs.
- Sesiones de Agent mode cortas con criterio de éxito claro.

**Gobernanza (admin, ciclo continuo):**
- Content Exclusion a nivel enterprise/org para build, generados, logs y secrets.
- Budgets por usuario/cost-center con alertas 75/90/100%.
- Default económico (GPT-4.1 / GPT-5 mini); premium restringido por política.
- Dashboard de AI Credits revisado semanalmente; OTel traces a Azure Monitor/Grafana.

## Ejercicio de cierre

Con el billing dashboard abierto (o el de un compañero que lleve días usando Copilot):

1. Identifica qué feature domina el consumo (chat, agente, code review, CLI).
2. Identifica el modelo que domina el consumo y pregúntate si las tareas que lo usan lo justifican.
3. Propón un cambio de configuración de este workshop que atacaría ese consumo (un `model` distinto en un agente, mover reglas a `applyTo`, apagar review automático en repos no críticos) y estima el efecto.

Si no hay datos disponibles, haz el ejercicio en seco con la sesión más larga de este workshop: cuenta los turnos y razona cuánto contexto viajó en el último.

## Referencias

**Fuentes de este módulo:**
- [Handbook: Optimización de tokens en GitHub Copilot](https://github.com/armandoblanco/github-copilot-token-optimization) (Armando Blanco, MIT) — guía completa de las cinco palancas, Content Exclusion y checklist.
- Paula Silva — [Token Optimization Handbook (deck)](https://agenticdevopsplatform.ai/decks/GitHubCopilotTokenOptimizationHandbook_Deck_v3_0_1_2026-06-15_multi.html) y [Workshop de otimização de tokens (deck)](https://agenticdevopsplatform.ai/decks/GitHubCopilotTokenOptimizationWorkshop_Deck_v2_0_0_2026-05-29_multi.html).
- Anthropic — ["Effective context engineering for AI agents"](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents) (context rot, attention budget) y ["Advanced tool use"](https://www.anthropic.com/engineering/advanced-tool-use) (tool bloat).

**Documentación oficial:**
- [Anuncio de la transición a facturación por uso](https://github.blog/news-insights/company-news/github-copilot-is-moving-to-usage-based-billing/) (GitHub Blog).
- [Models and pricing](https://docs.github.com/en/copilot/reference/copilot-billing/models-and-pricing) y [documentación de billing](https://docs.github.com/en/copilot/concepts/billing-and-payments) para cifras vigentes.
- [Managing context in Copilot CLI](https://docs.github.com/en/copilot/concepts/agents/copilot-cli/context-management) (comandos `/context`, `/compact`, `/usage`).
- [Content exclusion](https://docs.github.com/en/copilot/concepts/context/content-exclusion) y [cómo configurarlo](https://docs.github.com/en/copilot/how-tos/configure-content-exclusion/exclude-content-from-copilot).
- [Requests legacy (planes anuales)](https://docs.github.com/en/copilot/reference/copilot-billing/request-based-billing-legacy/copilot-requests) si tu organización aún tiene usuarios en el modelo anterior.

## Cierre

La versión honesta de este módulo en tres líneas: el costo vive en el contexto que se re-envía, no en lo que escribes; la palanca más grande es qué modelo hace qué tarea; y las capas del workshop, bien usadas, ya son la arquitectura de ahorro. Las cinco palancas se apilan hasta ~60-70% de ahorro sin cambiar lo que entregas. Optimizar tokens no es una disciplina aparte de personalizar Copilot: es personalizarlo bien.

## Siguiente

Vuelve al [Módulo 7: Cuándo usar qué](07-cuando-usar-que.md) para el árbol de decisión completo, ahora con hooks y costos en mente, o al [índice del workshop](../README.md).
