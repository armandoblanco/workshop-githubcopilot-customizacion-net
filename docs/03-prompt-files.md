# Módulo 3: Prompt files reutilizables

Tiempo estimado: 20 minutos.

## Objetivo

Crear prompt files que se pueden invocar como slash commands en Copilot Chat. Vamos a construir dos:

1. `/nuevo-endpoint` que genera un endpoint completo siguiendo las convenciones del proyecto.
2. `/revisar-prestamo` que aplica una checklist específica al código de cálculo de préstamos.

## Conceptos clave

Un prompt file vive en `.github/prompts/<nombre>.prompt.md` y aparece como `/nombre` en el menú de slash commands del chat. Es el mismo lugar donde aparecen los comandos built-in (`/explain`, `/tests`).

A diferencia de las instrucciones, que se cargan automáticamente en cada turno, un prompt file solo se carga cuando el usuario lo invoca explícitamente. Esto los hace ideales para tareas repetitivas que no quieres tener que reescribir cada vez.

Diferencia con un agente: un prompt file es una receta para un turno; un agente es una persona que se queda activa varios turnos. Si necesitas mantener un rol durante toda una conversación, usa agente. Si necesitas disparar una acción puntual, usa prompt file.

## Paso 3.1: Anatomía de un prompt file

```markdown
---
description: Descripción que se muestra en el menú de slash commands
agent: agent
argument-hint: "[descripción del endpoint]"
---

# Instrucciones del prompt

Aquí va el cuerpo del prompt. Puedes usar variables como ${input:nombre}
y referenciar archivos con [enlace](../instructions/algo.instructions.md).
```

Campos relevantes del frontmatter:

| Campo | Para qué sirve |
|-------|----------------|
| `description` | Texto en el menú de slash commands |
| `agent` | Qué agente usar para ejecutar el prompt (`agent` es el built-in) |
| `argument-hint` | Texto guía que se ve en la caja del chat cuando seleccionas el comando |
| `tools` | Lista de tools permitidas (heredada del agente si no se especifica) |
| `model` | Modelo específico para este prompt. Por ejemplo `Claude Opus 4.5 (copilot)` |

## Paso 3.2: Crear `/nuevo-endpoint`

Crea `.github/prompts/nuevo-endpoint.prompt.md`:

```markdown
---
description: Genera un endpoint Minimal API completo para Contoso Banco
agent: agent
argument-hint: "<descripción del endpoint en lenguaje natural>"
---

# Generar nuevo endpoint

Vas a crear un endpoint nuevo en el proyecto Contoso Banco - Servicio de Préstamos.

## Lo que recibes

El usuario describe el endpoint en lenguaje natural. Por ejemplo:
"un GET que devuelve los préstamos de un cliente filtrados por estado".

## Pasos a seguir

1. Determina el grupo al que pertenece el endpoint (préstamos, clientes, etc.).
   Si no existe el archivo `Endpoints/<Grupo>Endpoints.cs`, créalo siguiendo
   el patrón de los archivos existentes en `Endpoints/`.

2. Define la ruta y verbo HTTP siguiendo convenciones REST.

3. Define los DTOs de entrada y salida como `record` en `Models/`. Reutiliza
   los existentes si ya hay uno apropiado.

4. Implementa el endpoint usando `MapGet`, `MapPost`, `MapPut` o `MapDelete`
   según corresponda.

5. Aplica todas las convenciones definidas en
   `.github/instructions/endpoints.instructions.md`:
   - `.WithName()`, `.WithTags()`, `.WithOpenApi()` siempre.
   - `.Produces<T>()` para cada código de respuesta posible.
   - Validación en el endpoint, no en el servicio.
   - 422 para errores de regla de negocio.

6. Si el endpoint requiere lógica nueva en algún servicio, agrégala en
   `Services/`. No la metas inline en el endpoint.

7. Genera al menos un test de integración en
   `ContosoBanco.Loans.Tests/Endpoints/` que cubra el camino feliz y un error
   de validación.

## Lo que NO debes hacer

- No agregar paquetes nuevos.
- No introducir Controllers.
- No crear documentación nueva fuera de los comentarios XML.
- No modificar endpoints existentes.

## Formato de salida

Antes de crear nada, muestra al usuario:
- Ruta y verbo HTTP propuestos.
- DTOs nuevos que vas a crear.
- Servicios que vas a tocar.
- Tests que vas a generar.

Espera confirmación. Si el usuario dice "adelante", procede a crear los archivos.
```

## Paso 3.3: Probarlo

En Copilot Chat escribe:

```
/nuevo-endpoint un GET /api/prestamos/cliente/{clienteId} que devuelva todos los préstamos activos de un cliente
```

Observa que Copilot:

1. Te muestra primero un plan (ruta, DTOs, archivos que va a tocar).
2. Espera tu confirmación.
3. Crea los archivos siguiendo las convenciones de `endpoints.instructions.md`.
4. Genera un test de integración correspondiente.

Si el modelo procede directamente sin pedir confirmación, refuerza con: "Recuerda mostrarme el plan primero". Es no determinista.

## Paso 3.4: Crear `/revisar-prestamo`

Este segundo prompt es más interesante porque mezcla referencia a archivos del repo con un argumento del usuario. Crea `.github/prompts/revisar-prestamo.prompt.md`:

```markdown
---
description: Revisa código relacionado con cálculo o lógica de préstamos
agent: agent
argument-hint: "<archivo o función específica a revisar>"
model: Claude Opus 4.5 (copilot)
---

# Revisión específica de lógica de préstamos

Revisa el archivo o función indicada y aplica esta checklist en orden.
Reporta cada hallazgo con: severidad (alta, media, baja), archivo y línea,
descripción del problema, y propuesta de corrección.

## Reglas de dominio (severidad alta si fallan)

1. Todo monto se maneja con `decimal`. Si encuentras `double` o `float` para
   dinero, es severidad alta.

2. Las tasas se almacenan como decimal anual. Si ves operaciones que parecen
   tratar 18 como 18% en lugar de 0.18, es severidad alta.

3. La fórmula de amortización francesa correcta es:
   `cuota = monto * (i * (1+i)^n) / ((1+i)^n - 1)`
   donde `i` es la tasa mensual (tasa anual / 12) y `n` es el plazo en meses.
   Cualquier desviación es severidad alta.

4. Validar rangos antes de calcular: monto entre 1000 y 5000000, plazo entre
   6 y 60 meses. Si el cálculo se ejecuta sin validar, severidad alta.

## Reglas de implementación (severidad media)

5. División entre cero protegida. Si la tasa es 0, la fórmula falla.

6. Redondeo explícito al final del cálculo, no en pasos intermedios. Usar
   `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.

7. Operaciones financieras intermedias en `decimal`, no convertir a `double`
   en ningún punto.

## Reglas de estilo y mantenibilidad (severidad baja)

8. Nombres en español según las convenciones del repo.

9. Métodos públicos con comentario XML que describan parámetros, valor de
   retorno y excepciones esperadas.

10. Tests cubriendo: tasa cero, plazo mínimo, plazo máximo, monto mínimo,
    monto máximo, monto fuera de rango.

## Formato del reporte

Devuelve una tabla en Markdown con columnas: Severidad, Archivo:línea,
Hallazgo, Propuesta. Al final, un resumen de cuántos hallazgos hay por
severidad.

Si no hay hallazgos en alguna categoría, dilo explícitamente. No inventes
problemas para llenar el reporte.
```

Notas sobre este prompt:

- Especifica `model: Claude Opus 4.5 (copilot)` porque la revisión requiere razonamiento numérico cuidadoso.
- La instrucción "no inventes problemas para llenar el reporte" es importante. Sin ella, los modelos tienden a producir hallazgos triviales para parecer útiles.

## Paso 3.5: Probar `/revisar-prestamo`

Selecciona el archivo `Services/PrestamoServicio.cs` en VS Code y en el chat escribe:

```
/revisar-prestamo Services/PrestamoServicio.cs
```

El starter incluye errores deliberados en este archivo. Copilot debería detectar al menos:

- Un uso incorrecto de `double` en lugar de `decimal`.
- Una fórmula de amortización mal implementada.
- Validaciones de rango faltantes.

Si Copilot reporta menos hallazgos de los esperados, ejecuta el comando una segunda vez. La variabilidad es real. Si reporta hallazgos que no existen, refuerza la instrucción "no inventes" y vuelve a correrlo.

## Paso 3.6: Argumentos en prompt files

Los prompt files pueden recibir argumentos de varias formas:

- Texto libre después del comando: `/nuevo-endpoint <descripción>`. El texto pasa al cuerpo del prompt como contexto.
- Variables `${input:nombre}` en el cuerpo: cuando el modelo encuentra esa variable, pregunta al usuario su valor.
- Selección de código activa: si tienes texto seleccionado en el editor, se pasa como contexto automáticamente.

Para este workshop nos quedamos con texto libre, que es lo más simple y lo que más usarás en la práctica.

## Decisión: prompt file vs función inline en el chat

¿Vale la pena crear un prompt file para algo que harás dos veces?

No. Los prompt files cuestan mantenimiento. La regla práctica:

- Si la tarea se va a repetir más de cinco veces y la instrucción tiene más de tres líneas: prompt file.
- Si es una tarea ad hoc o el prompt es de una línea: inline en el chat.

No conviertas todo en prompt file. Un repo con 30 prompt files es peor que un repo con tres bien diseñados, porque nadie sabe cuál usar.

## Siguiente

[Módulo 4: Custom agents](04-custom-agents.md)
