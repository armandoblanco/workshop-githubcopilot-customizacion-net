# Módulo 7: Cuándo usar qué

Tiempo estimado: 5-10 minutos.

Este módulo no es práctico, es de cierre. Resume los trade-offs de las cuatro capas de personalización para que tomes decisiones rápidas en proyectos reales.

## Árbol de decisión

```
¿La regla aplica siempre, en cada interacción?
├── Sí, en todo el repo                  → copilot-instructions.md
├── Sí, pero solo a cierto tipo de archivo → .github/instructions/*.instructions.md (con applyTo)
└── No, es a demanda
    │
    ├── Es una receta puntual (un turno)
    │   ├── Sin recursos extra            → .github/prompts/*.prompt.md
    │   └── Con scripts o assets           → .github/skills/<nombre>/SKILL.md
    │
    └── Es un rol persistente (toda la sesión)
        ├── Necesito restringir tools       → .github/agents/*.agent.md
        ├── Necesito modelo específico       → .github/agents/*.agent.md (campo model)
        └── Necesito handoffs entre roles   → .github/agents/*.agent.md (campo handoffs)
```

## Tabla comparativa

| Capa | Trigger | Persistencia | Restringe tools | Recursos extra | Handoffs |
|------|---------|--------------|-----------------|----------------|----------|
| Instrucciones repo-wide | Automático en cada turno | Toda la sesión | No | No | No |
| Instrucciones path-specific | Automático cuando matchea applyTo | Toda la sesión | No | No | No |
| Prompt files | `/nombre` manual | Un turno | Vía agente | No | No |
| Custom agents | Selección en dropdown | Toda la sesión hasta que cambies | Sí | No | Sí |
| Agent skills | `/nombre` manual o automático por relevancia | Un turno o flujo | Hereda del agente | Sí (scripts, archivos) | No |

## Anti-patrones comunes

**Crear un agente para cada rol del equipo.**

Si tienes "agente backend", "agente frontend", "agente mobile", "agente devops"... estás usando agentes como categorías, no como roles. Un agente vale la pena cuando tiene un comportamiento distintivo (tools restringidas, modelo distinto, handoff a otro agente). Si solo cambia el lenguaje, una instrucción path-specific es mejor.

**Convertir cada prompt frecuente en prompt file.**

Los prompt files cuestan mantenimiento. Si en seis meses nadie los actualiza, se vuelven obsoletos y dañan más que ayudan. Empieza con el chat normal. Cuando la misma frase la escribas por décima vez, ahí sí créalo.

**Skills sin scripts ni assets.**

Una skill con solo un `SKILL.md` y nada más es un prompt file disfrazado. La razón de existir de las skills es empacar lógica con sus recursos. Si no tienes recursos, no inflas la abstracción.

**Instrucciones con todo dentro.**

Un `copilot-instructions.md` con 200 líneas garantiza que las reglas del final se ignoran en code review (límite de 4000 caracteres) y que el chat consume contexto innecesario en cada turno. Modulariza con archivos `*.instructions.md` por path o por tema.

**No usar restricciones de tools en agentes.**

Si tu custom agent tiene acceso a todas las tools, estás dejando sobre la mesa el feature más valioso del agente. Un revisor con permiso de editar es un mal revisor por diseño. Un planificador con permiso de ejecutar comandos es un mal planificador por diseño.

## Cuántos archivos de cada tipo es razonable

Estas son cifras que he visto funcionar en repos reales. No son ley.

| Capa | Mínimo útil | Bandera amarilla | Bandera roja |
|------|-------------|------------------|--------------|
| Instrucciones | 1 | 5+ | 10+ archivos en `instructions/` |
| Prompt files | 0 | 8+ | 15+ |
| Custom agents | 0 | 5+ | 8+ |
| Skills | 0 | 4+ | 8+ |

Si tienes 12 prompt files, alguien va a estar perdido buscando cuál usar. Considera consolidar.

Si tienes ocho custom agents, probablemente tres están duplicando funcionalidad. Audita.

Si tienes diez archivos de instrucciones, probablemente puedes consolidar varios usando `applyTo` con patrones más amplios.

## Mantenimiento a lo largo del tiempo

Lo que hace que estos archivos se vuelvan basura técnica:

1. Nadie los actualiza cuando cambia el código que describen.
2. Las reglas que en algún momento fueron críticas dejan de aplicar pero nadie las quita.
3. Se contradicen entre sí porque crecieron en silos.

Una práctica que funciona: revisión trimestral de la carpeta `.github/`, en el mismo PR donde se actualiza el `copilot-instructions.md`. El que toca una regla, revisa todas las del archivo. Diez minutos cada tres meses evita que los archivos se vuelvan irrelevantes.

## Recursos para seguir explorando

- [Awesome Copilot](https://github.com/github/awesome-copilot): instrucciones, agentes y skills contribuidas por la comunidad. Útil como referencia de qué hace otra gente, pero no copies sin entender.
- [Documentación de personalización en VS Code](https://code.visualstudio.com/docs/copilot/customization/overview): la fuente más actualizada cuando algo cambia.
- [Documentación de Copilot code review](https://docs.github.com/copilot/using-github-copilot/code-review/using-copilot-code-review): la lista oficial de límites y comportamientos.
- [Estándar de Agent Skills](https://agentskills.io): especificación abierta del formato de skills.

## Cierre

El valor de personalizar Copilot no está en tener muchos archivos, está en que las cosas correctas estén en los lugares correctos. Empieza simple: un `copilot-instructions.md` y un par de prompt files. Mide qué te molesta repetir, y solo entonces sube en complejidad.
