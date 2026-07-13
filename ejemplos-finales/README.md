# Ejemplos finales

Esta carpeta contiene la versión resuelta de todos los archivos de
personalización que se construyen durante el workshop. No está pensada para
ser copiada tal cual al inicio: el valor del workshop está en construir
cada archivo guiado por Copilot.

Sin embargo, hay dos casos legítimos de uso:

1. **Recuperación**: un participante que se atrasa copia el archivo que le
   falta y sigue el ritmo del grupo.
2. **Referencia**: comparar tu versión con esta después de terminar un módulo
   para ver qué hiciste distinto.

## Contenido

```
.github/
├── copilot-instructions.md              Módulo 2
├── instructions/
│   ├── tests.instructions.md            Módulo 2
│   ├── endpoints.instructions.md        Módulo 2
│   └── code-review.instructions.md      Módulo 6
├── prompts/
│   ├── nuevo-endpoint.prompt.md         Módulo 3
│   ├── revisar-prestamo.prompt.md       Módulo 3
│   └── revisar-endpoints.prompt.md      Módulo 6
├── agents/
│   ├── arquitecto.agent.md              Módulo 4
│   ├── implementador.agent.md           Módulo 4
│   ├── revisor-seguridad.agent.md       Módulo 4
│   ├── planificador.agent.md            Módulo 4.1 (subagent worker)
│   ├── codificador.agent.md             Módulo 4.1 (subagent worker)
│   ├── auditor.agent.md                 Módulo 4.1 (subagent worker)
│   ├── feature-builder.agent.md         Módulo 4.1 (coordinador)
│   └── revisor-360.agent.md             Módulo 4.1 (revisión en paralelo)
├── skills/
│   ├── scoring-crediticio/              Módulo 5
│   │   ├── SKILL.md
│   │   ├── validar-scoring.csx
│   │   └── ejemplos/
│   │       └── caso-scoring.json
│   ├── reportes-regulatorios/           Módulo 5.1
│   │   ├── SKILL.md
│   │   └── references/
│   │       ├── plantilla-cartera-vencida.md
│   │       ├── plantilla-buro-mensual.md
│   │       └── plantilla-operaciones-inusuales.md
│   ├── caveman/                         Módulo 5.1 (comunidad, adaptada)
│   │   └── SKILL.md
│   └── markitdown/                      Módulo 5.1 (comunidad, adaptada)
│       └── SKILL.md
└── hooks/
    ├── auditoria.json                   Módulo 8
    ├── politica-comandos.json           Módulo 8
    └── scripts/
        ├── bloquear-comandos.sh
        └── bloquear-comandos.ps1
```

Las skills `caveman` y `markitdown` son adaptaciones originales en español
de proyectos de la comunidad ([JuliusBrussee/caveman](https://github.com/JuliusBrussee/caveman)
y [microsoft/markitdown](https://github.com/microsoft/markitdown)). Antes de
copiar cualquier skill de terceros a un repo real, lee el checklist de
seguridad del [módulo 5.1](../docs/05_01-skills-avanzadas.md).

## Cómo copiar al starter

Asumiendo que estás en la raíz de `starter/`.

**macOS / Linux**:

```bash
# Copiar todo
cp -r ../ejemplos-finales/.github .

# O copiar solo un archivo específico
mkdir -p .github/agents
cp ../ejemplos-finales/.github/agents/arquitecto.agent.md .github/agents/
```

**Windows (PowerShell)**:

```powershell
# Copiar todo
Copy-Item -Recurse -Force ../ejemplos-finales/.github .

# O copiar solo un archivo específico
New-Item -ItemType Directory -Force -Path .github/agents | Out-Null
Copy-Item ../ejemplos-finales/.github/agents/arquitecto.agent.md .github/agents/
```

Después de copiar, abre el editor de Chat Customizations
(`Chat: Open Chat Customizations`) y verifica que los archivos aparecen
con check verde. Si aparecen con error, el frontmatter YAML tiene algún
problema de indentación o formato.

## Advertencia

Los modelos y tools referenciados en los archivos (`Claude Opus 4.5`,
`Claude Sonnet 4.5`, `edit`, `search/codebase`, etc.) pueden cambiar de
nombre con el tiempo. Si algún agente no funciona, revisa el log del chat
de Copilot para ver si reporta un tool o modelo inexistente, y actualiza
el archivo al nombre vigente.
