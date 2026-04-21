# Módulo 1: Setup y selección de modelo

Tiempo estimado: 10 minutos.

## Objetivo

Tener todos los participantes con el código base corriendo, Copilot configurado con un modelo Claude (Sonnet o Opus) y el menú de personalización de chat localizado.

## Paso 1.1: Verificar versiones

```bash
dotnet --version    # 8.x
code --version      # Reciente, idealmente 1.95+
git --version
```

Si `dotnet --version` no devuelve algo en la familia 8.x, instala el SDK desde https://dot.net antes de seguir.

## Paso 1.2: Clonar el código base

```bash
git clone https://github.com/armandoblanco/workshop-githubcopilot-customizacion-net.git workshop
cd workshop/starter
code .
```

Cuando VS Code abra la solución, el C# Dev Kit puede pedirte permiso para restaurar paquetes. Acepta.

Verifica que la API arranca:

```bash
dotnet run --project ContosoBanco.Loans.Api
```

Abre `http://localhost:5000/swagger` y confirma que ves los endpoints de `/api/prestamos`. Si todo carga, mata el proceso con `Ctrl+C`.

## Paso 1.3: Inicializar el repositorio Git de tu workspace

El starter no tiene `.git` propio. Inicialízalo:

```bash
cd starter
git init
git add .
git commit -m "Estado inicial del workshop"
```

Esto te permitirá ver con claridad qué archivos cambias en cada módulo.

## Paso 1.4: Localizar el selector de modelo

1. Abre Copilot Chat con `Ctrl+Shift+I` (Windows/Linux) o `Cmd+Shift+I` (macOS).
2. En la parte inferior de la caja de texto del chat, busca el selector de modelo. Es una pequeña etiqueta que muestra el modelo actual.
3. Cámbialo a `Claude Sonnet 4.5 (copilot)`.

Si no ves Claude en la lista, ve a `https://github.com/settings/copilot/features` y habilita los modelos de Anthropic. Si tu organización los bloquea, usa GPT-5 o Gemini 2.5 Pro: la mayoría de los ejercicios funcionan, pero los handoffs entre agentes y los skills se aprovechan mejor con Claude.

## Paso 1.5: Localizar el selector de agente

En la misma caja de chat, hay un segundo selector que muestra el agente activo (por defecto, **Agent**). Ahí es donde aparecerán los **custom agents** que crees en el módulo 4.

También tienes un menú de slash commands: escribe `/` en la caja de chat y observa qué aparece. Notas la lista de comandos disponibles. Aún no hay nada custom porque no hemos creado prompts ni skills propios.

## Paso 1.6: Abrir el editor de Chat Customizations

VS Code tiene un editor unificado para gestionar instrucciones, prompts, agentes y skills:

1. Abre la paleta de comandos con `Ctrl+Shift+P` (Windows/Linux) o `Cmd+Shift+P` (macOS).
2. Ejecuta `Chat: Open Chat Customizations`.

Verás pestañas para **Instructions**, **Prompts**, **Agents** y **Skills**. Por ahora todas están vacías. Las iremos llenando módulo por módulo.

## Verificación final

Antes de pasar al módulo 2, asegúrate de que:

- La API corre y Swagger se ve.
- El chat de Copilot está usando un modelo Claude.
- El editor de Chat Customizations abre sin errores.

Si algo falla, revisa la pestaña **Output > GitHub Copilot Chat** para ver el log y compara contra los pre-requisitos del README.

## Una observación crítica

VS Code y la extensión de Copilot se actualizan muy rápido. Los nombres y ubicaciones exactas de los selectores de modelo, agente y configuración cambian con frecuencia. Si lo que ves no coincide exactamente con lo descrito aquí, el cambio probablemente está en el [changelog de VS Code](https://code.visualstudio.com/updates). No asumas que el workshop está roto: asume que la UI cambió.

## Siguiente

[Módulo 2: Instrucciones personalizadas](02-instrucciones-personalizadas.md)
