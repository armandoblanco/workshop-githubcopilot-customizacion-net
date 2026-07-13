# Módulo 5: Agent Skills

Tiempo estimado: 20 minutos.

## Objetivo

Crear una skill que enseñe a Copilot a **clasificar el riesgo crediticio** de un solicitante aplicando la tabla de scoring del banco y sus reglas duras. La skill incluye un script de validación que Copilot puede correr para verificar su propia implementación.

Ojo: esto es a propósito **una tarea distinta** a lo que hiciste en los módulos anteriores. Las instrucciones (módulo 2) y los prompts (módulo 3) giraban alrededor de calcular la cuota y revisar el préstamo. Aquí la skill decide **si se aprueba o rechaza** al cliente, que es conocimiento procedimental nuevo: un modelo de puntaje más una capa de reglas que lo sobrescriben.

## Conceptos clave

Las skills son carpetas con un `SKILL.md` (formato YAML frontmatter más Markdown) que pueden incluir scripts, ejemplos y otros recursos. Son un estándar abierto: las skills que escribes funcionan en GitHub Copilot CLI, en VS Code y en el coding agent de la nube.

Las skills resuelven un problema que ni las instrucciones ni los prompts pueden: empacar **conocimiento procedimental con sus recursos asociados**. Una skill puede incluir scripts ejecutables, plantillas, ejemplos y cualquier archivo que la tarea necesite.

Diferencia importante con instrucciones y prompts:

| Aspecto | Instrucciones | Prompt files | Agent skills |
|---------|---------------|--------------|--------------|
| Carga | Siempre o por path | A demanda con `/` | A demanda o automática por relevancia |
| Recursos extra | No | No | Sí, scripts y archivos |
| Portabilidad | Solo VS Code y GitHub.com | Solo VS Code, Visual Studio, JetBrains | VS Code, CLI, coding agent (estándar abierto) |
| Tamaño típico | Pequeño | Pequeño-mediano | Mediano-grande con assets |

## Cómo carga Copilot una skill

Esta parte es importante porque define cómo escribir la skill. Copilot carga las skills en tres niveles:

1. **Discovery**: solo lee el frontmatter (`name` y `description`) de cada `SKILL.md` que encuentra. Esto es prácticamente gratis en términos de contexto.

2. **Instrucciones**: cuando determina (o el usuario invoca con `/`) que una skill es relevante, carga el cuerpo del `SKILL.md` en contexto.

3. **Recursos**: solo carga archivos auxiliares (scripts, ejemplos) cuando los referencia desde el `SKILL.md` y los necesita.

La consecuencia práctica: el `description` debe ser específico. Si dice "ayuda con cálculos", Copilot no va a saber cuándo cargarla. Si dice "clasifica el riesgo crediticio de un solicitante, calcula su score y aplica las reglas de aprobación o rechazo", la activación se vuelve confiable.

> Los comandos de creación de archivos difieren entre shells. A continuación incluimos la versión **macOS/Linux** (bash/zsh) y **Windows (PowerShell)**.

## Paso 5.1: Crear la estructura

Crea la carpeta y los archivos.

**macOS / Linux**:

```bash
mkdir -p .github/skills/scoring-crediticio/ejemplos
touch .github/skills/scoring-crediticio/SKILL.md
touch .github/skills/scoring-crediticio/validar-scoring.csx
touch .github/skills/scoring-crediticio/ejemplos/caso-scoring.json
```

**Windows (PowerShell)**:

```powershell
New-Item -ItemType Directory -Force -Path .github/skills/scoring-crediticio/ejemplos | Out-Null
New-Item -ItemType File -Force -Path .github/skills/scoring-crediticio/SKILL.md | Out-Null
New-Item -ItemType File -Force -Path .github/skills/scoring-crediticio/validar-scoring.csx | Out-Null
New-Item -ItemType File -Force -Path .github/skills/scoring-crediticio/ejemplos/caso-scoring.json | Out-Null
```

Importante: el nombre de la carpeta debe coincidir exactamente con el campo `name` del frontmatter. Si la carpeta es `scoring-crediticio`, `name` debe ser `scoring-crediticio`. Si no coinciden, la skill no se carga.

## Paso 5.2: Escribir el SKILL.md

El contenido completo está en [`ejemplos-finales/.github/skills/scoring-crediticio/SKILL.md`](../ejemplos-finales/.github/skills/scoring-crediticio/SKILL.md). Las ideas que lo hacen una buena skill:

- **Un `description` específico** que enumera cuándo activarse (evaluar, clasificar, aprobar/rechazar una solicitud, calcular el score).
- **La metodología completa en dos capas.** No basta con una suma de puntos: primero un score numérico y después reglas duras que lo pueden sobrescribir. Ese matiz es lo que convierte un dato en conocimiento procedimental que vale la pena empacar.
- **Casos de prueba con valores esperados**, para que Copilot valide su implementación en vez de "confiar" en que quedó bien.

El corazón de la skill es esta estructura de decisión. Score base 500, se suman
puntos por cuatro factores (relación cuota/ingreso, historial, antigüedad
laboral, relación monto/ingreso), se recorta a `[300, 850]` y se mapea a una
banda A/B/C/D. Después, dos reglas duras pueden **degradar** la decisión:

- **RD-1**: historial `malo` → Rechazado, sin importar el score.
- **RD-2**: DTI > 0.45 → nunca aprobado automático (tope banda C).

Copia el `SKILL.md` de referencia a tu `.github/skills/scoring-crediticio/` o
reconstrúyelo guiado por Copilot. Lo importante es que quede la tabla de puntaje,
las bandas y las dos reglas duras.

## Paso 5.3: Crear el script de validación

El script vive en [`ejemplos-finales/.github/skills/scoring-crediticio/validar-scoring.csx`](../ejemplos-finales/.github/skills/scoring-crediticio/validar-scoring.csx). Implementa las dos capas (puntaje + reglas duras) y comprueba los tres casos estándar:

```csharp
// Extracto: la capa 2 es la que suele faltar en una implementación ingenua.
int score = Math.Clamp(suma, 300, 850);

string banda =
    score >= 720 ? "A" :
    score >= 660 ? "B" :
    score >= 600 ? "C" : "D";

if (historial == "malo")
    banda = "D";                                   // RD-1
else if (dti > 0.45m && (banda == "A" || banda == "B"))
    banda = "C";                                   // RD-2
```

Cópialo completo desde la referencia. Si los participantes no tienen `dotnet-script` instalado, pueden instalarlo con `dotnet tool install -g dotnet-script`. Alternativa: el script lo puede ejecutar Copilot directamente desde el chat usando la tool de comandos.

## Paso 5.4: Crear el ejemplo de salida

El archivo `ejemplos/caso-scoring.json` documenta la salida esperada de los tres casos, incluido el desglose de puntos. El caso 3 es el importante: score 720 (banda A por puntaje) pero **decisión final Rechazado** porque la regla dura RD-1 lo degrada. Cópialo desde [`ejemplos-finales/.github/skills/scoring-crediticio/ejemplos/caso-scoring.json`](../ejemplos-finales/.github/skills/scoring-crediticio/ejemplos/caso-scoring.json).

Ese caso es la mejor prueba de la skill: una implementación que solo suma puntos y olvida las reglas duras **aprueba** el caso 3. La skill correcta lo rechaza.

## Paso 5.5: Verificar que la skill aparece

1. Abre el editor de Chat Customizations (`Chat: Open Chat Customizations`).
2. Ve a la pestaña **Skills**.
3. Debería aparecer `scoring-crediticio` con un check verde.

Si no aparece, las causas comunes son:

- El nombre de la carpeta no coincide con el campo `name` del frontmatter.
- El frontmatter YAML está mal formado.
- El archivo no se llama exactamente `SKILL.md` (cuidado con mayúsculas).

## Paso 5.6: Probar la skill

Hay dos maneras de invocar una skill: explícitamente con `/` o dejando que Copilot la cargue automáticamente cuando determina que es relevante.

**Invocación explícita:**

```
/scoring-crediticio Implementa un método EvaluarSolicitud en PrestamoServicio.cs que reciba ingreso, cuota, historial, antigüedad y monto, y devuelva score, banda y decisión. Valídalo con los casos estándar.
```

Verifica que:

1. Copilot implementa la tabla de puntaje completa.
2. Implementa **las dos reglas duras**, no solo la suma.
3. Usa `decimal` en el DTI y la relación de monto.
4. Ejecuta el script de validación al final.
5. Reporta los resultados de los tres casos, incluido el caso 3 (rechazado por RD-1).

**Invocación automática:**

```
Un cliente gana 80000 al mes, la cuota sería 12000, tiene historial malo y 60 meses de antigüedad, pide 100000. ¿Se le aprueba el préstamo?
```

Si la skill está bien escrita, Copilot debería decidir cargarla por su cuenta basándose en el `description`. Verás en el log del chat algo como "Loaded skill: scoring-crediticio", y la respuesta correcta es **Rechazado** por historial malo, aunque el score sea 720.

Si no la carga automáticamente, el `description` no es suficientemente específico. Iterá sobre él hasta que la activación sea confiable.

## Paso 5.7: Decisión de invocación

Las skills tienen dos flags útiles en el frontmatter:

- `user-invocable: false`: la skill no aparece en el menú `/` pero Copilot la carga automáticamente cuando la considera relevante. Útil para skills que son "conocimiento de fondo" que no quieres que el usuario tenga que disparar manualmente.

- `disable-model-invocation: true`: la skill solo se carga cuando el usuario la invoca con `/`. Copilot nunca la carga por su cuenta. Útil para skills costosas o sensibles donde quieres control explícito.

La combinación por defecto (ambos sin especificar) es ambas cosas: aparece en `/` y se puede cargar automáticamente. Es lo correcto para la mayoría de casos.

## Trade-offs honestos

¿Cuándo conviene una skill y cuándo es overkill?

**Conviene skill cuando:**
- Hay conocimiento procedimental no obvio (fórmulas, protocolos, secuencias de pasos).
- Necesitas distribuir scripts o plantillas con la lógica.
- La misma capacidad debe estar disponible en VS Code, CLI y coding agent.
- Quieres que Copilot pueda validar su propio trabajo.

**No conviene skill cuando:**
- Es una regla de naming o convención: usa instrucciones.
- Es una receta de un solo turno: usa prompt file.
- Es un rol con personalidad: usa custom agent.

Una señal de que tienes overkill: tu skill tiene 50 líneas de Markdown sin scripts ni assets. Eso debería ser un prompt file.

## Siguiente

[Módulo 5.1: Skills avanzadas y skills de la comunidad](05_01-skills-avanzadas.md) _(opcional, extiende este módulo con references, flags de invocación y skills de terceros como caveman y markitdown)_

O salta directo a [Módulo 6: Code review](06-code-review.md).
