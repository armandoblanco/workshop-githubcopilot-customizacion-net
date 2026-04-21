# Workshop avanzado de personalización de GitHub Copilot con .NET

Workshop de dos horas centrado en las cuatro capas de personalización de GitHub Copilot: **instrucciones**, **prompt files**, **custom agents** y **agent skills**. Cierra con el uso de **code review** asistido por Copilot, tanto en VS Code como en GitHub.com.

El escenario es un servicio de préstamos para Contoso Banco. La API ya viene implementada de forma mínima en `starter/`, así que el tiempo se invierte en personalizar Copilot, no en escribir CRUDs.

## A quién va dirigido

Este workshop asume que ya conoces:

- Los modos Ask, Edit y Agent de Copilot Chat en VS Code.
- Cómo crear un `.github/copilot-instructions.md` básico.
- Comandos slash como `/explain`, `/tests`, `/fix`.
- C# y .NET 8 a nivel intermedio.

Si nunca has trabajado con GitHub Copilot en .NET, primero pasa por el [workshop básico](https://github.com/armandoblanco/workshop-githubcopilot-basico-net) y luego vuelve aquí.

## Índice del workshop

Sigue los módulos en orden. Cada uno enlaza al siguiente al final.

**Antes de empezar**
- **[📍 Mapa de personalización](docs/00-mapa-personalizacion.md)** · 5 min · qué se carga automáticamente y qué invocas a mano.

**Ruta principal** _(2 horas)_
1. **[Setup y selección de modelo](docs/01-setup.md)** · 10 min · Claude Sonnet/Opus y chat customizations.
2. **[Instrucciones personalizadas](docs/02-instrucciones-personalizadas.md)** · 20 min · repo-wide y path-specific con `applyTo`.
3. **[Prompt files](docs/03-prompt-files.md)** · 20 min · recetas reutilizables con argumentos.
4. **[Custom agents](docs/04-custom-agents.md)** · 30 min · roles con tools restringidas y handoffs.
5. **[Agent skills](docs/05-agent-skills.md)** · 20 min · paquetes con scripts y recursos.
6. **[Code review con Copilot](docs/06-code-review.md)** · 20 min · en VS Code y en pull requests de GitHub.

**Extras**
- **[4.1 Subagents](docs/04_01-subagents.md)** _(opcional, +25-30 min)_ · orquestación automática y revisión en paralelo. Extiende el módulo 4.
- **[7. Cuándo usar qué](docs/07-cuando-usar-que.md)** · 5-10 min · árbol de decisión y anti-patrones. Ideal como cierre.
- **[Glosario](docs/glosario.md)** · referencia rápida de términos.

## Estructura del repositorio

```
.
├── README.md                    Este archivo
├── docs/                        Una sub-página por módulo
│   ├── 00-mapa-personalizacion.md
│   ├── 01-setup.md
│   ├── 02-instrucciones-personalizadas.md
│   ├── 03-prompt-files.md
│   ├── 04-custom-agents.md
│   ├── 04_01-subagents.md
│   ├── 05-agent-skills.md
│   ├── 06-code-review.md
│   ├── 07-cuando-usar-que.md
│   └── glosario.md
├── starter/                     Código base que cada participante clona
│   ├── ContosoBanco.Loans.sln
│   ├── ContosoBanco.Loans.Api/
│   └── ContosoBanco.Loans.Tests/
└── ejemplos-finales/            Versión de referencia con todo construido
    └── .github/
        ├── copilot-instructions.md
        ├── instructions/
        ├── prompts/
        ├── agents/
        └── skills/
```

La carpeta `ejemplos-finales/` existe para que un participante que se atrase pueda copiar los archivos resueltos y seguir el ritmo del grupo. No la usen como copy/paste por defecto: el aprendizaje está en construir cada archivo guiado por Copilot.

## Pre-requisitos

- VS Code 1.95 o superior.
- Extensión **GitHub Copilot** y **GitHub Copilot Chat** (versiones recientes que ya incluyen el menú de **Custom Agents** y **Agent Skills**).
- **C# Dev Kit**.
- **.NET 8 SDK** (`dotnet --version` debe devolver 8.x).
- Cuenta de GitHub con licencia Copilot que tenga **Claude Sonnet 4.5** y/o **Claude Opus 4.5** habilitados en el selector de modelos.
- Git y una terminal funcional.

## Modelo recomendado

Las features que vamos a usar (instrucciones path-specific, agentes con tools y handoffs, skills) funcionan mejor con modelos de razonamiento fuerte. Recomendado:

- **Claude Sonnet 4.5** para el flujo general.
- **Claude Opus 4.5** para los pasos donde haya razonamiento financiero o revisión de seguridad (el módulo 4 lo configura explícitamente para el agente de revisión).

GPT-5 funciona razonablemente, pero algunos ejercicios están diseñados pensando en cómo Claude maneja contexto largo y handoffs.

## Cómo usar este repositorio

1. Clona el repo.
2. Abre `starter/` en VS Code.
3. Lee el [Mapa de personalización](docs/00-mapa-personalizacion.md) (5 minutos) para entender qué se carga solo y qué no.
4. Avanza módulo por módulo empezando por [docs/01-setup.md](docs/01-setup.md). Cada módulo enlaza al siguiente al final.
5. Si te atrasas, copia el archivo correspondiente desde `ejemplos-finales/.github/` y continúa.

### Ruta rápida (por si te pierdes)

[Mapa](docs/00-mapa-personalizacion.md) → [Setup](docs/01-setup.md) → [Instrucciones](docs/02-instrucciones-personalizadas.md) → [Prompts](docs/03-prompt-files.md) → [Agentes](docs/04-custom-agents.md) → [Subagents](docs/04_01-subagents.md) _(opcional)_ → [Skills](docs/05-agent-skills.md) → [Code review](docs/06-code-review.md) → [Cuándo usar qué](docs/07-cuando-usar-que.md)

## Notas para el instructor

El módulo 4 (custom agents) y el módulo 5 (skills) son los más densos. Si el grupo va lento en los primeros módulos, comprime el módulo 6 (code review) y déjalo como demostración en vivo en lugar de práctica individual. La parte irrenunciable es que cada participante cree al menos un agente, una skill y un prompt file. Sin eso, el workshop pierde sentido.

No intentes cubrir todo el frontmatter de cada formato. Cubre lo mínimo viable y deja los campos avanzados (`hooks`, `mcp-servers`, `disable-model-invocation`) como lectura posterior en `docs/glosario.md`.

## Licencia

MIT.
