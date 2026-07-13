# Módulo 8: Hooks

Tiempo estimado: 25 minutos.

## Pre-requisito

Haber completado los módulos 2 (instrucciones) y 4 (custom agents). Los hooks se entienden mejor por contraste con lo que las instrucciones no pueden garantizar.

## Objetivo

Crear dos hooks para el repo de Contoso Banco: uno de auditoría que registra cada prompt enviado al agente, y uno de política que bloquea comandos peligrosos antes de que se ejecuten. Al final vas a entender cuándo un problema se resuelve con instrucciones y cuándo exige un hook.

## El problema que las instrucciones no resuelven

En el módulo 2 escribiste reglas como "nunca uses `double` en cálculos financieros". Funcionan la mayoría de las veces, pero son **probabilísticas**: el modelo puede ignorarlas, olvidarlas en sesiones largas o interpretarlas mal. Para convenciones de estilo eso es tolerable. Para "nunca ejecutes `DROP DATABASE` en este repo" no lo es.

Los **hooks** son la respuesta determinística: comandos de shell que tú defines y que se ejecutan en puntos fijos del ciclo de vida del agente. No dependen de que el modelo coopere. Un hook `preToolUse` que deniega un comando lo deniega siempre, sin importar qué tan convencido esté el agente de que era buena idea.

La distinción conceptual que ordena todo este módulo:

| | Instrucciones | Hooks |
|---|---|---|
| Naturaleza | Texto que el modelo interpreta | Código que el sistema ejecuta |
| Garantía | Probabilística | Determinística |
| Puede bloquear acciones | No | Sí (`preToolUse`) |
| Puede modificar el comportamiento del modelo | Sí | Indirectamente (inyectando contexto o denegando) |
| Costo de mantenimiento | Bajo | Medio (son scripts, se testean como scripts) |

## Dónde corren los hooks

Los hooks funcionan en tres superficies, y conviene tener claro cuál es cuál porque no son idénticas:

| Superficie | Dónde se configuran | Estado |
|---|---|---|
| **Copilot CLI** | `.github/hooks/*.json` del repo, más hooks personales en `~/.copilot/hooks/*.json` | Disponible, soporta todos los eventos |
| **Copilot cloud agent** (coding agent) | `.github/hooks/*.json` en la rama por defecto | Disponible |
| **VS Code** (modo agente del chat) | `.github/hooks/*.json`, más el comando `/hooks` en el chat para crearlos guiado | **Preview**: el formato puede cambiar |

El mismo archivo JSON en `.github/hooks/` sirve para las tres superficies, con matices: VS Code traduce internamente los nombres de evento a PascalCase (`preToolUse` lo lee como `PreToolUse`) y mapea `bash` a macOS/Linux y `powershell` a Windows. Tu organización puede tener los hooks deshabilitados por política; si nada carga, pregunta a tu admin antes de asumir que el JSON está mal.

## Los eventos disponibles

| Evento | Cuándo dispara | Uso típico |
|---|---|---|
| `sessionStart` | Al iniciar o retomar una sesión | Preparar entorno, registrar inicio para auditoría |
| `userPromptSubmitted` | Cada vez que el usuario envía un prompt | Log de auditoría, análisis de uso |
| `preToolUse` | Antes de que el agente use cualquier tool | **El más poderoso**: puede aprobar o denegar la ejecución |
| `postToolUse` | Después de que una tool se ejecutó | Formateo automático, escaneo de secretos en archivos tocados |
| `errorOccurred` | Cuando ocurre un error en la sesión | Notificaciones, métricas de errores |
| `sessionEnd` | Al terminar la sesión | Limpieza, archivar logs, notificar |

Copilot CLI agrega `agentStop` (el agente terminó de responder) y `permissionRequest` (intercepta el flujo de permisos, útil en modo pipe/CI donde no hay humano que apruebe). El cloud agent no dispara `permissionRequest`; ahí las decisiones de permiso se hacen con `preToolUse`.

Cada hook recibe un JSON por stdin con el contexto del evento (timestamp, cwd, y en los de tool: `toolName` y `toolArgs`), y puede responder con JSON por stdout para influir en el agente.

## El formato de configuración

Los hooks viven en archivos JSON dentro de `.github/hooks/`. El formato exige `version: 1` y un objeto `hooks` con arreglos por evento:

```json
{
  "version": 1,
  "hooks": {
    "sessionStart": [
      {
        "type": "command",
        "bash": "echo \"sesión iniciada $(date)\" >> /tmp/copilot.log",
        "powershell": "Add-Content -Path $env:TEMP/copilot.log -Value \"sesión iniciada $(Get-Date)\"",
        "timeoutSec": 5
      }
    ]
  }
}
```

Incluye siempre las dos claves, `bash` y `powershell`, para que el hook funcione en macOS, Linux y Windows: Copilot elige la correcta según el sistema del usuario. En un workshop con participantes en ambos sistemas esto no es opcional.

## Paso 8.1: Hook de auditoría de prompts

Contoso Banco necesita saber qué se le pide al agente en este repo (requisito típico de un banco: trazabilidad de herramientas que tocan código productivo). Crea `.github/hooks/auditoria.json`:

```json
{
  "version": 1,
  "hooks": {
    "userPromptSubmitted": [
      {
        "type": "command",
        "bash": "INPUT=$(cat); mkdir -p .copilot-logs; echo \"$INPUT\" | jq -c '{ts: .timestamp, prompt: .prompt}' >> .copilot-logs/audit.jsonl",
        "powershell": "$input_json = [Console]::In.ReadToEnd() | ConvertFrom-Json; New-Item -ItemType Directory -Force -Path .copilot-logs | Out-Null; @{ts=$input_json.timestamp; prompt=$input_json.prompt} | ConvertTo-Json -Compress | Add-Content .copilot-logs/audit.jsonl",
        "timeoutSec": 10
      }
    ]
  }
}
```

Agrega `.copilot-logs/` al `.gitignore` del starter: el log es local, no quieres prompts de desarrolladores en el historial de git.

Dos advertencias que en un banco no son teóricas:

- Los prompts pueden contener datos sensibles. En producción, este log iría a un sistema centralizado con control de acceso y retención, no a un archivo local, y con redacción de patrones sensibles antes de escribir.
- No registres jamás tokens ni credenciales que aparezcan en `toolArgs`. Si vas a loggear tool calls, registra metadata (nombre de tool, timestamp, decisión), no argumentos completos.

## Paso 8.2: Hook de política de comandos

Ahora el hook que justifica el módulo: bloquear patrones de comandos que nunca deberían auto-ejecutarse. Primero el script. Crea `.github/hooks/scripts/bloquear-comandos.sh`:

```bash
#!/bin/bash
# Deniega comandos peligrosos antes de que el agente los ejecute.
# Recibe el JSON del evento por stdin; responde JSON por stdout.

INPUT=$(cat)
TOOL=$(echo "$INPUT" | jq -r '.toolName // empty')

# Solo nos interesan ejecuciones de shell
if [ "$TOOL" != "bash" ] && [ "$TOOL" != "powershell" ]; then
  exit 0
fi

COMANDO=$(echo "$INPUT" | jq -r '.toolArgs // "{}" | fromjson | .command // empty' 2>/dev/null)

PATRONES='rm -rf /|rm -rf \.|drop database|drop table|database drop|curl .*\| *(bash|sh)|wget .*\| *(bash|sh)|iex *\(|git push --force'

if echo "$COMANDO" | grep -qiE "$PATRONES"; then
  echo '{"behavior": "deny", "message": "Comando bloqueado por política de Contoso Banco (CB-SEC-014). Si es legítimo, ejecútalo manualmente fuera del agente."}'
  exit 0
fi

exit 0
```

Y su equivalente `.github/hooks/scripts/bloquear-comandos.ps1` para Windows (misma lógica con `ConvertFrom-Json` y `-imatch`; la versión completa está en `ejemplos-finales/.github/hooks/scripts/`).

Luego la configuración, `.github/hooks/politica-comandos.json`:

```json
{
  "version": 1,
  "hooks": {
    "preToolUse": [
      {
        "type": "command",
        "bash": "bash .github/hooks/scripts/bloquear-comandos.sh",
        "powershell": "pwsh -File .github/hooks/scripts/bloquear-comandos.ps1",
        "timeoutSec": 10
      }
    ]
  }
}
```

Dale permisos de ejecución en macOS/Linux: `chmod +x .github/hooks/scripts/bloquear-comandos.sh`.

Nota `toolArgs`: llega como **string JSON dentro del JSON**, por eso el script hace `fromjson` antes de leer `.command`. Es el tropiezo más común escribiendo hooks.

## Paso 8.3: Probar

Los hooks se testean como cualquier script: por fuera primero, con el agente después.

**Por fuera** (macOS/Linux):

```bash
echo '{"timestamp":1704614400000,"cwd":".","toolName":"bash","toolArgs":"{\"command\":\"rm -rf /tmp/x\"}"}' | bash .github/hooks/scripts/bloquear-comandos.sh
```

Debe imprimir el JSON de denegación. Cambia el comando a `dotnet build` y debe salir sin imprimir nada.

**Con el agente**: reinicia la sesión (los hooks se cargan al inicio) y en modo agente pide algo que dispare un comando prohibido, por ejemplo "ejecuta rm -rf /tmp/pruebas para limpiar" (nota que el patrón bloquea rutas absolutas y el directorio actual, no cualquier `rm -rf`; ajusta los patrones a la política real de tu equipo). El agente debe recibir la denegación con tu mensaje y buscar otra vía o reportarlo. Pide después un `dotnet build` normal y verifica que pasa.

En VS Code puedes ver qué hooks cargaron y su entrada/salida en **Output > GitHub Copilot Chat Hooks** y con `Developer: Show Agent Debug Logs`.

## Semántica de fallos: léela dos veces

Esta parte es donde los hooks muerden si no la conoces:

- Para la mayoría de eventos, si tu script truena o excede el timeout, se registra el error y **la sesión continúa**. Un hook de auditoría roto no detiene al agente.
- Para `preToolUse` la lógica es distinta: un script que truena o sale con código distinto de cero **deniega la tool** (fail-closed), pero un **timeout deja pasar** (fail-open), para que un hook lento o colgado no bloquee silenciosamente todo el agente.

Consecuencia práctica: un `preToolUse` con un bug de sintaxis bloquea todas las tools del agente y parece que "Copilot se rompió". Si el agente deja de poder hacer nada después de que agregaste un hook, el hook es el sospechoso número uno.

## Consideraciones de seguridad honestas

Los hooks ejecutan comandos con tus permisos, así que aplican las mismas reglas que le pedirías a cualquier automatización:

- **El agente puede editar tus scripts de hooks** si tiene tool de edición y los scripts viven en el repo. Eso significa que podría reescribir la política que lo restringe. En VS Code, configura `chat.tools.edits.autoApprove` para que ediciones a scripts de hooks requieran aprobación manual. Sin esto, tu `preToolUse` es una puerta con la llave puesta.
- Valida y sanitiza el input del hook: llega del agente, no de una fuente confiable.
- Hooks corren **sincrónicamente y bloquean al agente**. Mantenlos por debajo de 5 segundos; logging pesado va a segundo plano.
- Un hook en el repo aplica a todo el que clone y use el agente ahí. Es una decisión de equipo, revisable en PR como cualquier otra pieza de infraestructura.

## Cuándo usar hooks y cuándo no

**Conviene hook cuando:**
- La regla debe cumplirse siempre, sin excepción probabilística (políticas de seguridad, auditoría, compliance).
- Quieres integrar el ciclo del agente con sistemas externos (logging centralizado, notificaciones).
- Necesitas post-procesamiento determinístico (formatear archivos tocados, escanear secretos).

**No conviene hook cuando:**
- Es una preferencia de estilo o convención: instrucciones (módulo 2). Un hook para "usa nombres en español" es matar moscas a cañonazos y agrega latencia a cada evento.
- La lógica requiere juicio semántico ("bloquea cambios que rompan la arquitectura"): eso no se regexea, es trabajo para un agente revisor (módulo 4) o code review (módulo 6).
- Todavía no tienes el problema. Un repo de workshop con cinco hooks especulativos es mantenimiento sin retorno.

La combinación madura en un repo bancario real: instrucciones para el estilo, agentes con tools restringidas para los roles, hooks para las líneas rojas, y code review como red final. Cuatro capas, cuatro garantías distintas.

## Referencias

- [Conceptos de hooks](https://docs.github.com/en/copilot/concepts/agents/hooks) y [referencia completa de eventos y payloads](https://docs.github.com/en/copilot/reference/hooks-reference) en GitHub Docs.
- [Hooks en Copilot CLI para ejecución conforme a políticas](https://docs.github.com/en/copilot/tutorials/copilot-cli-hooks) (tutorial oficial del caso de política, similar al paso 8.2).
- [Agent hooks en VS Code](https://code.visualstudio.com/docs/agent-customization/hooks) (Preview, incluye el detalle de PascalCase y los logs de debug).
- Ejemplos de la comunidad en [Awesome Copilot](https://github.com/github/awesome-copilot/blob/main/docs/README.hooks.md).

## Siguiente

[Módulo 9: Optimización de tokens](09-optimizacion-tokens.md)
