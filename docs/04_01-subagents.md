# Módulo 4.1: Subagents y orquestación automática

Tiempo estimado: 25-30 minutos.

## Pre-requisito

Haber completado el módulo 4 (custom agents básicos con handoffs).

## Objetivo

Convertir el flujo de tres agentes que armaste en el módulo 4 (arquitecto,
implementador, revisor de seguridad) en un flujo realmente autónomo donde un
agente coordinador orquesta a los demás sin que tengas que hacer clic entre
pasos. También cubrirás el patrón de revisión multi-perspectiva en paralelo.

## El problema con los handoffs

Los handoffs que armaste en el módulo 4 funcionan, pero requieren que el
usuario haga clic en el botón al final de cada turno para pasar al siguiente
agente. Esto es deseable cuando quieres revisar el resultado intermedio y
decidir si continuar, pero es fricción innecesaria cuando el flujo está bien
definido y solo quieres ver el resultado final.

Hay un segundo mecanismo, distinto e independiente, llamado **subagents**.
Con subagents, un agente principal invoca a otro agente como si fuera una
tool: pasa un prompt, espera el resultado, y continúa con su trabajo. Sin
clic intermedio.

## Diferencias entre handoffs y subagents

|  | Handoffs | Subagents |
|---|---|---|
| Quién dispara | El usuario hace clic | El agente principal decide |
| Contexto | Se transfiere al siguiente agente | Cada subagent tiene su propio context window aislado |
| Quién mantiene control | Cambia al siguiente agente | El agente principal sigue al mando |
| Paralelismo | No | Sí, varios subagents en paralelo |
| Usado para | Flujos guiados con punto de revisión humana | Orquestación autónoma, exploración paralela |

Las dos cosas pueden coexistir en el mismo agente. Un agente puede invocar
subagents internamente y al final del turno mostrar handoffs para que el
usuario decida el siguiente paso global.

## Conceptos clave de subagents

Tres ideas que conviene tener claras antes de construir nada:

**Aislamiento de contexto.** Cada subagent arranca con un context window
limpio. No ve la conversación completa del agente principal. Solo ve el
prompt que el agente principal le pasa y el contenido que sus tools le
permiten leer. Esto es útil para tareas donde no quieres contaminar el
contexto principal con detalles de implementación o exploración fallida.

**Sincronía con paralelismo.** Cada llamada a subagent es síncrona (el
agente principal se bloquea esperando el resultado), pero pueden lanzarse
varios subagents en paralelo. Si pides "analiza estos cuatro aspectos en
paralelo", VS Code lanza los cuatro al mismo tiempo y espera a que todos
terminen.

**Especialización por agente.** Por defecto los subagents heredan modelo y
tools del agente principal, pero si los apuntas a un custom agent específico
(con `name: <nombre>`), el subagent usa el modelo y las tools de ese custom
agent. Aquí está el valor real: el coordinador puede usar un modelo barato
para orquestar, mientras delega la lógica pesada a workers especializados
con modelos más capaces.

## Tools necesarias

Para que un agente pueda invocar subagents, necesita la tool `agent` (o
`runSubagent`, dependiendo de tu versión de VS Code) en su lista de tools.
Sin esa tool, el agente físicamente no puede crear subagents aunque las
instrucciones le pidan hacerlo.

## Patrón 1: Coordinator con workers secuenciales

Este es el equivalente automático de tu flujo del módulo 4. Un agente
coordinador que delega secuencialmente a tres workers, sin handoffs ni
clics intermedios.

### Paso 1: Crear los workers

Los workers son custom agents normales pero con dos propiedades adicionales
en el frontmatter que los marcan como "para uso interno":

- `user-invocable: false` los oculta del dropdown de selección de agentes en
  el chat. El usuario no los puede seleccionar manualmente, solo se acceden
  como subagents.
- (Opcional) `disable-model-invocation: true` previene que cualquier agente
  los invoque como subagent. No lo usas para los workers porque justamente
  quieres que el coordinador los invoque.

Crea `.github/agents/planificador.agent.md`:

```markdown
---
name: planificador
description: Worker interno de planificación. No invocar directamente.
user-invocable: false
tools: ['search/codebase', 'search/usages', 'read']
model: Claude Opus 4.5 (copilot)
---

# Worker: Planificador

Recibes una solicitud de feature y devuelves un plan de implementación
en Markdown. No editas archivos ni ejecutas comandos.

El plan debe tener:

- Resumen en 1-2 frases.
- Archivos a crear o modificar.
- Pasos atómicos numerados (cada paso debe dejar el código compilando).
- Tests a escribir.
- Riesgos identificados.

Sé conciso. El plan es input para otro worker, no para un humano.
```

Crea `.github/agents/codificador.agent.md`:

```markdown
---
name: codificador
description: Worker interno de implementación. No invocar directamente.
user-invocable: false
tools: ['edit', 'search/codebase', 'read', 'runCommands']
model: Claude Sonnet 4.5 (copilot)
---

# Worker: Codificador

Recibes un plan de implementación y lo ejecutas paso a paso. Después de
cada cambio significativo, ejecutas `dotnet build` para confirmar que
compila.

Sigues las convenciones de `.github/copilot-instructions.md` y los archivos
en `.github/instructions/`.

Al terminar devuelves:
- Lista de archivos modificados.
- Resultado de `dotnet build` y `dotnet test`.
- Cualquier paso del plan que no pudiste completar y por qué.

No tomas decisiones arquitectónicas. Si el plan es ambiguo, marca el
problema y devuelve sin implementar esa parte.
```

Crea `.github/agents/auditor.agent.md`:

```markdown
---
name: auditor
description: Worker interno de auditoría de seguridad. No invocar directamente.
user-invocable: false
tools: ['read', 'search/codebase', 'search/usages']
model: Claude Opus 4.5 (copilot)
---

# Worker: Auditor

Recibes una lista de archivos modificados y haces auditoría de seguridad
y correctitud de dominio sobre esos cambios.

Reglas que aplicas, en este orden:

1. Validación de entrada en endpoints.
2. Uso correcto de `decimal` para dinero.
3. Fórmula de amortización francesa correcta.
4. Validación de rangos antes de cálculos (monto 1000-5000000, plazo 6-60).
5. División entre cero protegida.
6. Manejo de excepciones específico (no `catch (Exception)` silencioso).
7. Sin información sensible en código o logs.

Devuelves un reporte por severidad: Crítica, Alta, Media, Baja. Si una
categoría no tiene hallazgos, lo dices explícitamente. No inventas
problemas.

No editas archivos. Solo lectura.
```

### Paso 2: Crear el coordinador

Ahora el agente que orquesta a los tres. Crea
`.github/agents/feature-builder.agent.md`:

```markdown
---
name: feature-builder
description: Construye features completas delegando a workers internos. Un comando, todo el flujo.
tools: ['agent', 'read']
agents: ['planificador', 'codificador', 'auditor']
model: Claude Sonnet 4.5 (copilot)
---

# Coordinador: Feature Builder

Tu rol es orquestar la construcción de una feature de extremo a extremo.
No escribes código ni planificas tú mismo. Delegas a workers especializados.

## Flujo

Para cada solicitud de feature que recibas:

1. Invoca al subagent **planificador** pasándole la descripción de la
   feature. Espera su plan.

2. Muestra el plan al usuario en una sola línea de resumen y pregunta si
   procede. Si el usuario dice que sí (o no responde explícitamente que
   no), continúa.

3. Invoca al subagent **codificador** pasándole el plan completo. Espera
   el resultado.

4. Si el codificador reporta que `dotnet build` o `dotnet test` falló, no
   sigas. Reporta el error al usuario y termina.

5. Invoca al subagent **auditor** pasándole la lista de archivos
   modificados. Espera el reporte.

6. Sintetiza un resumen final con tres bloques:
   - Qué se construyó (en 2-3 líneas).
   - Resultado del build y los tests.
   - Hallazgos del auditor por severidad.

## Reglas

- No invoques manualmente otros agentes que no estén en tu lista `agents`.
- No edites archivos directamente. Para eso está el codificador.
- Si el planificador devuelve un plan vago o incompleto, vuelve a invocarlo
  con feedback específico antes de pasar al codificador.
- Máximo dos iteraciones entre planificador y codificador. Si después de
  eso no converges, detente y reporta al usuario.
```

Las dos líneas clave del frontmatter:

- `tools: ['agent', 'read']`: el coordinador puede crear subagents (`agent`)
  y leer archivos para entender el contexto, pero no puede editar.
- `agents: ['planificador', 'codificador', 'auditor']`: lista cerrada de
  workers que puede invocar. Si no especificas esto, podría invocar
  cualquier custom agent disponible, lo cual es inseguro.

### Paso 3: Probar el flujo

1. En el dropdown de agentes, selecciona **feature-builder**.
2. Verifica que **planificador**, **codificador** y **auditor** NO aparecen
   en el dropdown (porque tienen `user-invocable: false`).
3. Escribe en el chat: "Necesito agregar un endpoint POST /api/prestamos/cancelar/{id} que cambie el estado de un préstamo a cancelado solo si está en estado solicitado".
4. Observa cómo el coordinador, sin pedirte clic intermedio:
   - Invoca al planificador y obtiene el plan.
   - Invoca al codificador, que escribe y compila el código.
   - Invoca al auditor, que revisa.
   - Te devuelve un resumen único.

En el chat verás cada subagent como una "tool call colapsable". Si haces
clic en cada una, ves el detalle de qué hizo cada worker internamente.

## Patrón 2: Workers paralelos para revisión multi-perspectiva

El segundo patrón explota la capacidad de paralelismo. En lugar de tres
revisiones secuenciales, lanzas tres revisiones en paralelo cada una
enfocada en un aspecto distinto. Cuando todas terminan, el coordinador
sintetiza.

Lo interesante de este patrón: como dice la documentación oficial, los
workers no necesitan ser custom agents separados. El coordinador puede
definir el rol de cada subagent en su prompt directamente. Esto reduce el
número de archivos.

Crea `.github/agents/revisor-360.agent.md`:

```markdown
---
name: revisor-360
description: Revisión multi-perspectiva en paralelo. Cuatro lentes a la vez.
tools: ['agent', 'read', 'search/codebase']
model: Claude Sonnet 4.5 (copilot)
---

# Coordinador: Revisor 360

Tu rol es coordinar cuatro revisiones simultáneas e independientes del
código indicado, y consolidar los hallazgos en un reporte único priorizado.

## Cómo trabajas

Cuando recibas una solicitud de revisión, lanza estos cuatro subagents
**en paralelo**. Cada uno corre con context window aislado y enfoque
distinto:

1. **Subagent de correctitud**: revisa lógica, edge cases, manejo de
   tipos, off-by-one. Modelo: Claude Opus 4.5.

2. **Subagent de seguridad de dominio**: revisa uso de decimal, fórmulas
   financieras, validación de rangos, división entre cero. Aplica las
   reglas de `.github/instructions/code-review.instructions.md`.

3. **Subagent de calidad**: revisa nombres, duplicación, complejidad
   ciclomática alta, métodos demasiado largos.

4. **Subagent de arquitectura**: revisa adherencia a patrones del repo,
   ubicación correcta de la lógica (endpoints vs servicios), respeto a
   las convenciones de Minimal API.

Cada subagent debe devolver un reporte con: hallazgos por severidad,
referencia a archivo y línea, y propuesta de corrección.

## Síntesis final

Cuando los cuatro terminen, produces un único reporte consolidado con esta
estructura:

### Resumen ejecutivo
Cuántos hallazgos por severidad, agregados de las cuatro revisiones.

### Hallazgos críticos y altos
Tabla con: lente que lo detectó, archivo:línea, descripción, propuesta.

### Hallazgos medios y bajos
Lista resumida.

### Conflictos entre lentes
Si dos subagents reportan recomendaciones opuestas (ejemplo: uno pide
extraer un método, otro pide inlinear), márcalo explícitamente. No
intentes resolverlo tú; deja que el desarrollador decida.

### Lo que se vio bien
Si alguna lente reportó que todo está limpio en su área, recógelo aquí.

## Lo que NO haces

- No editas archivos.
- No haces follow-up a los subagents (un solo turno cada uno).
- No agregas perspectivas extra fuera de las cuatro definidas.
```

### Probar el patrón paralelo

1. Selecciona **revisor-360** en el dropdown.
2. Escribe: "Revisa el archivo `Services/PrestamoServicio.cs`".
3. Observa en el chat: deberías ver cuatro subagent tool calls aparecer
   simultáneamente (no en secuencia). Cada uno con su propia barra de
   progreso.
4. Cuando todos terminen, el coordinador sintetiza el reporte único.

El paralelismo es lo que hace este patrón valioso. Hacer cuatro revisiones
en serie con cuatro modelos distintos puede tomar dos o tres minutos. En
paralelo es el tiempo de la más lenta, no la suma.

## Cuándo usar cada patrón

**Coordinator + workers secuenciales** (patrón 1):

Cuando hay una dependencia natural entre pasos. El codificador necesita el
plan del planificador. El auditor necesita la lista de archivos del
codificador. No tiene sentido lanzarlos en paralelo porque cada uno
necesita el output del anterior.

**Coordinator + workers paralelos** (patrón 2):

Cuando los workers son independientes y miran lo mismo desde ángulos
distintos. La revisión de seguridad no necesita el resultado de la
revisión de calidad. Pueden correr juntos y consolidarse al final.

**Handoffs (módulo 4)**:

Cuando quieres mantener al humano en el loop entre pasos. Útil en flujos
nuevos donde no confías todavía en el output intermedio, o en flujos que
involucran decisiones de negocio que requieren juicio humano.

Los tres mecanismos pueden coexistir en el mismo repo. No son alternativos;
son herramientas distintas para problemas distintos.

## Limitaciones honestas

**El paralelismo no es gratis.** Cada subagent consume premium requests por
separado. Lanzar cuatro revisiones en paralelo cuesta cuatro veces más que
una sola revisión. En proyectos con presupuesto de Copilot ajustado, esto
importa.

**No hay debugging fácil.** Cuando un coordinador con cinco subagents falla,
encontrar cuál subagent dio el output incorrecto requiere expandir cada
tool call y leer su contexto. No es complicado pero es tedioso.

**El coordinador puede malinterpretar el output del subagent.** Los
subagents devuelven texto, no estructuras. Si tu coordinador espera un JSON
y el worker devuelve Markdown, el coordinador puede confundirse. Para flujos
críticos, instruye explícitamente al worker sobre el formato esperado.

**`user-invocable` vs `user-invokable`.** La documentación oficial de VS
Code usa ambas escrituras en páginas distintas (`user-invocable` en la doc
de Custom Agents, `user-invokable` en la doc de Subagents). Hasta donde he
podido confirmar, ambas funcionan en versiones recientes pero la canónica
parece ser `user-invocable`. Si tu archivo no carga bien, prueba la otra.

**Los nombres de tools cambian.** En algunas versiones de VS Code la tool
para invocar subagents se llama `agent`, en otras `runSubagent`. Si el
coordinador no logra invocar workers, revisa el log del chat de Copilot:
te dirá si la tool no existe.

## Refactor sugerido para tu repo

Si vienes del módulo 4 con tres agentes (arquitecto, implementador,
revisor-seguridad) usando handoffs, no necesitas tirarlos. Puedes mantener
los dos modelos:

- Los tres agentes originales del módulo 4 siguen disponibles para flujos
  donde quieres revisar entre pasos.
- Los nuevos workers (planificador, codificador, auditor) y el coordinador
  feature-builder dan un flujo automatizado para tareas estándar.
- El coordinador revisor-360 da un análisis multi-perspectiva cuando lo
  necesitas.

Conviven en el mismo `.github/agents/`. El usuario elige cuál usar según el
contexto.

## Siguiente

Vuelve al [Módulo 5: Agent Skills](05-agent-skills.md) o al
[Módulo 7: Cuándo usar qué](07-cuando-usar-que.md) para ver dónde encajan
los subagents en el árbol de decisión completo.