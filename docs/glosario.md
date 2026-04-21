# Glosario

Términos que aparecen en el workshop, en orden alfabético.

## Agent (modo)

Modo de operación de Copilot Chat donde el asistente puede modificar archivos y ejecutar comandos automáticamente. Distinto de un **custom agent**, que es una persona especializada definida por el usuario.

## Agent Skill

Carpeta con un `SKILL.md` y opcionalmente scripts y recursos, que enseña a Copilot una capacidad específica. Es un estándar abierto que funciona en VS Code, Copilot CLI y el coding agent de la nube.

## Ask (modo)

Modo de Copilot Chat donde el asistente solo responde preguntas sin modificar archivos. Útil para exploración y aprendizaje.

## applyTo

Campo del frontmatter en archivos `*.instructions.md` que define un patrón glob para limitar a qué archivos aplica la instrucción.

## Background agent

Agente que corre en segundo plano (terminal o nube) ejecutando tareas asignadas. Distinto del agente de chat interactivo.

## Chat Customizations editor

Vista de VS Code (`Chat: Open Chat Customizations`) que centraliza la gestión de instrucciones, prompts, agentes y skills.

## Chatmode

Nombre antiguo de los **custom agents**. Si tienes archivos `.chatmode.md`, conviene renombrarlos a `.agent.md`.

## Coding agent (Copilot)

Agente autónomo de GitHub que puede recibir issues asignados, hacer cambios en una rama y abrir un PR. Vive en `github.com/copilot/agents`. No es lo mismo que el modo agente del chat.

## copilot-instructions.md

Archivo en `.github/` con instrucciones repo-wide que se aplican en cada interacción de Copilot dentro del repo.

## Custom agent

Persona especializada definida en `.github/agents/<nombre>.agent.md` con sus propias instrucciones, tools restringidas, modelo preferido y handoffs opcionales.

## Edit (modo)

Modo de Copilot Chat donde el usuario selecciona archivos y pide ediciones específicas. Más control que Agent, menos automático.

## Frontmatter

Bloque YAML al principio de archivos Markdown delimitado por `---`. Define metadata como nombre, descripción, herramientas, modelo.

## Handoff

Botón que aparece al final del turno de un custom agent y permite pasar al siguiente agente del flujo con contexto preservado y un prompt pre-rellenado.

## Instrucciones path-specific

Archivos `.github/instructions/*.instructions.md` con un campo `applyTo` que define a qué archivos aplican. Permiten reglas específicas por lenguaje, capa o tipo de archivo.

## MCP (Model Context Protocol)

Protocolo abierto que permite a Copilot conectarse a servidores externos para acceder a herramientas adicionales (ejemplo: GitHub MCP server, Filesystem MCP, etc.). Configurable por agente.

## Plan (modo)

Modo donde Copilot genera un plan antes de ejecutar. La ejecución requiere aprobación paso a paso. En la versión actual, este modo se reemplazó frecuentemente con custom agents que tienen tools restringidas a lectura.

## Premium request

Llamada al modelo que cuenta contra una cuota mensual incluida en planes pagos de Copilot. Modos como Agent y modelos avanzados (Claude Opus, GPT-5) consumen premium requests.

## Prompt file

Archivo `.github/prompts/<nombre>.prompt.md` que define una receta reutilizable invocable como slash command (`/nombre`) en Copilot Chat.

## Slash command

Comando que empieza con `/` en la caja de chat. Pueden ser built-in (`/explain`, `/tests`, `/fix`, `/doc`) o definidos por el usuario vía prompt files o skills.

## SKILL.md

Archivo de definición de una agent skill. Vive en `.github/skills/<nombre>/SKILL.md`. El nombre de la carpeta debe coincidir con el campo `name` del frontmatter.

## Tools (en custom agents)

Lista de capacidades que un agente puede usar. Ejemplos: `read`, `edit`, `runCommands`, `search/codebase`, `web/fetch`. Restringir las tools de un agente es la forma más fuerte de controlar su comportamiento.
