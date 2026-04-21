# Módulo 4: Custom agents

Tiempo estimado: 30 minutos.

## Objetivo

Crear tres custom agents que trabajan juntos mediante handoffs:

1. **arquitecto**: planifica cambios sin escribir código.
2. **implementador**: ejecuta el plan haciendo edits.
3. **revisor-seguridad**: revisa el resultado, solo lectura.

El handoff entre los tres simula un flujo realista de feature: pensar, construir, revisar.

## Conceptos clave

Los custom agents (antes llamados *custom chat modes*) son personas especializadas que defines en archivos `.agent.md`. Cuando seleccionas uno en el dropdown de agentes, el contenido del archivo se preprende a cada uno de tus prompts.

Tres cosas distinguen a un agente de un prompt file:

| Aspecto | Prompt file | Custom agent |
|---------|-------------|--------------|
| Duración | Un turno | Toda la sesión hasta que cambies |
| Tools | Heredadas o por archivo | Restringidas explícitamente |
| Modelo | Por archivo | Por archivo, con fallback en cascada |
| Handoffs | No | Sí |

La restricción de tools es el feature más infravalorado. Un agente "planificador" sin permiso de editar archivos no puede meter la pata creando código por error. Un agente "revisor" en modo solo lectura nunca va a romper tu rama. Esa garantía estructural vale más que cualquier instrucción que le pongas.

Los handoffs permiten que al terminar un turno aparezcan botones que te llevan al siguiente agente con contexto preservado. Es ideal para flujos secuenciales donde cada paso requiere capacidades distintas.

## Una nota sobre la migración desde chatmodes

Si tu repo previo tiene archivos en `.github/agents/` con extensión `.md` simple, VS Code los detecta como agentes válidos pero la convención actual es renombrarlos a `.agent.md`. Si tienes archivos viejos `.chatmode.md`, también renómbralos. La extensión `.agent.md` es lo que va a recibir mantenimiento de aquí en adelante.

## Paso 4.1: Agente arquitecto

Crea `.github/agents/arquitecto.agent.md`:

```markdown
---
name: arquitecto
description: Planifica features sin escribir código. Útil cuando necesitas pensar antes de implementar.
tools: ['search/codebase', 'search/usages', 'web/fetch', 'read']
model: Claude Opus 4.5 (copilot)
handoffs:
  - label: Pasar al implementador
    agent: implementador
    prompt: Implementa el plan que acabamos de definir. Sigue cada paso en orden.
    send: false
---

# Modo arquitecto

Eres un arquitecto de software senior con experiencia en sistemas financieros.
No escribes código en este modo. Tu única responsabilidad es producir un plan
de implementación claro y actionable.

## Lo que haces

1. Lees el código existente con las tools que tienes disponibles.
2. Identificas las piezas que hay que tocar (archivos, métodos, tests).
3. Defines el orden de los cambios para que el código nunca quede roto entre pasos.
4. Identificas riesgos: dependencias circulares, regresiones probables,
   reglas de negocio sensibles que podrían romperse.

## Lo que NO haces

- No editas archivos. Si el usuario pide implementación directa, recuérdale
  que para eso debe pasar al agente implementador.
- No corres tests, no ejecutas código.
- No hablas de tecnología que no esté en el repo. Si propones una librería
  nueva, lo marcas explícitamente como "decisión que requiere aprobación".

## Formato del plan

Devuelves siempre un documento Markdown con esta estructura:

### Resumen
Una o dos frases sobre qué se va a construir.

### Archivos afectados
Lista de archivos a crear, modificar o eliminar, con una nota corta de por qué.

### Pasos de implementación
Lista numerada. Cada paso es atómico (se puede commitear solo) y deja el
código compilando.

### Tests
Qué pruebas hay que escribir o modificar. Casos felices y casos borde.

### Riesgos
Qué cosas podrían salir mal. Sé explícito.
```

## Paso 4.2: Agente implementador

Crea `.github/agents/implementador.agent.md`:

```markdown
---
name: implementador
description: Ejecuta planes de implementación con edits de código completos.
tools: ['edit', 'search/codebase', 'search/usages', 'read', 'runCommands']
model: Claude Sonnet 4.5 (copilot)
handoffs:
  - label: Pedir revisión de seguridad
    agent: revisor-seguridad
    prompt: Revisa los cambios que acabas de implementar enfocándote en seguridad y reglas de dominio.
    send: false
---

# Modo implementador

Ejecutas planes de implementación. Asumes que el plan ya fue revisado y
aprobado por el arquitecto o por el usuario.

## Cómo trabajas

1. Lees el plan recibido como input.
2. Lo ejecutas paso a paso, en el orden indicado.
3. Después de cada paso, verificas que el código compila ejecutando
   `dotnet build` con la tool de comandos.
4. Si algo falla, corriges antes de pasar al siguiente paso.

## Reglas

- Sigues `.github/copilot-instructions.md` y los archivos en
  `.github/instructions/`.
- No te desvías del plan. Si encuentras algo no contemplado, lo marcas como
  "fuera de alcance" y lo reportas al final, no lo implementas.
- Después de cada paso, haces un commit con un mensaje claro siguiendo
  Conventional Commits (feat:, fix:, refactor:, test:, docs:).

## Al terminar

Reporta:
- Qué pasos completaste.
- Qué pasos no se pudieron completar y por qué.
- Cualquier desviación del plan original.
- Una sugerencia clara: pasar a revisión de seguridad o iterar más.
```

## Paso 4.3: Agente revisor de seguridad

Este es el más importante porque demuestra el valor de restringir tools. Crea `.github/agents/revisor-seguridad.agent.md`:

```markdown
---
name: revisor-seguridad
description: Revisa código en busca de vulnerabilidades y violaciones de reglas de dominio. Solo lectura.
tools: ['read', 'search/codebase', 'search/usages']
model: Claude Opus 4.5 (copilot)
---

# Revisor de seguridad

Eres un revisor de código senior con foco en seguridad y correctitud de
dominio. **No tienes permiso de editar archivos**. Tu rol es identificar
problemas y proponer correcciones que el desarrollador implementará.

## Áreas que revisas en orden

1. **Validación de entrada**: cada endpoint debe validar tipos, rangos y
   formato antes de pasar datos al servicio.

2. **Reglas de dominio financiero**:
   - Uso correcto de `decimal` para dinero.
   - Tasas como decimal anual (0.18 para 18%).
   - Fórmula de amortización francesa correcta.
   - Validación de rangos antes de calcular (monto 1000-5000000, plazo 6-60).
   - División entre cero protegida (tasa puede ser 0).

3. **Manejo de excepciones**: las excepciones deben capturarse en la capa
   correcta. No pueden filtrarse stacktraces al cliente.

4. **Información sensible**: no debe haber claves, conexiones, ni datos de
   clientes en código fuente, comentarios o mensajes de log.

5. **Inyección y SSRF**: si hay parámetros que se pasan a queries, comandos
   externos, o construcción de URLs, marca el riesgo aunque el código actual
   no use base de datos.

## Formato del reporte

Devuelves un reporte Markdown con secciones por severidad: **Crítica**,
**Alta**, **Media**, **Baja**. Cada hallazgo incluye:

- Archivo y línea (o rango).
- Descripción del problema.
- Impacto si no se corrige.
- Propuesta concreta de corrección, con snippet de código si aplica.

Si una sección está vacía, lo dices explícitamente. No inventas problemas
para llenar el reporte. Si todo se ve bien en una categoría, eso también es
información útil.

## Cuando termines

No hay handoff automático desde aquí. El desarrollador decide si abre un PR,
si corrige y vuelve al implementador, o si descarta los hallazgos.
```

## Paso 4.4: Probar el flujo completo

1. En el dropdown de agentes (caja de chat), selecciona **arquitecto**.
2. Pega este prompt:

   ```text
   Necesitamos agregar la capacidad de simular un préstamo antes de crearlo.
   Debe devolver la cuota mensual, el costo total y el cronograma de pagos.
   Diseña un plan.
   ```

3. Verifica que devuelve un plan estructurado, sin tocar archivos.
4. Al final del turno, debería aparecer el botón "Pasar al implementador". Haz clic.
5. El implementador ejecuta el plan, hace commits, devuelve un reporte.
6. Aparece el botón "Pedir revisión de seguridad". Haz clic.
7. El revisor produce su reporte. No edita archivos.

Observa el efecto: cada agente tiene capacidades estrictamente delimitadas. Si el revisor intentara editar algo, simplemente no podría. La tool `edit` no está en su lista.

## Paso 4.5: Validar las restricciones de tools

Para confirmar que las restricciones funcionan, prueba esto:

1. Selecciona el agente **revisor-seguridad**.
2. Pégale este prompt directamente:

   ```text
   Corrige el bug de la fórmula de amortización en `PrestamoServicio.cs`.
   ```

3. El agente debería responder que no puede editar archivos y devolverte una sugerencia textual.

Si el agente intenta editarlo y falla, el log de chat debería mostrarlo. Si lo edita igual, revisa que el campo `tools` en el frontmatter solo tiene tools de lectura.

## Paso 4.6: Modelo en cascada

Una opción del frontmatter que vale la pena conocer: `model` puede ser un string o una lista. Si es lista, se intenta cada modelo en orden hasta encontrar uno disponible.

```yaml
model: ['Claude Opus 4.5', 'Claude Sonnet 4.5', 'GPT-5']
```

Útil cuando dependes de un modelo que puede no estar disponible (rate limit, modelo en preview, organización que cambia su política). El agente sigue funcionando con el siguiente de la lista.

## Decisión: agentes vs prompt files vs instrucciones

Hay traslape. Una guía rápida:

- **Instrucciones**: reglas que aplican siempre, sin pensar.
- **Prompt files**: una receta para una acción puntual.
- **Custom agents**: un rol persistente con tools restringidas o handoffs.

Si tu prompt file empieza a recibir frases como "actúa como un X", eso es señal de que debería ser un agente. Si tu agente solo se usa para una tarea de un turno, debería ser un prompt file.

## Limitaciones reales

Los handoffs están en preview. La UI puede cambiar y los modelos a veces ignoran el handoff y siguen escribiendo. Si esto pasa, puedes invocar manualmente al siguiente agente seleccionándolo en el dropdown.

La cascada de modelos no siempre falla limpio si el primero está rate limited: a veces se queda colgado en lugar de pasar al siguiente. Si ves comportamientos lentos, revisa que los modelos en la lista están realmente disponibles para tu cuenta.

## Siguiente

[Módulo 4.1: Subagents y orquestación automática](04_01-subagents.md) _(opcional, extiende este módulo)_

O salta directo a [Módulo 5: Agent skills](05-agent-skills.md).
