# Módulo 5.1: Skills avanzadas y skills de la comunidad

Tiempo estimado: 20-25 minutos (opcional, extiende el módulo 5).

## Pre-requisito

Haber completado el [módulo 5](05-agent-skills.md). Ahí construiste `scoring-crediticio`, una skill con un script de validación. Este módulo cubre lo que quedó fuera: el patrón de **referencias con carga progresiva**, los flags de invocación en la práctica, y cómo evaluar e instalar skills escritas por terceros.

## Objetivo

Tres cosas concretas:

1. Crear una skill (`reportes-regulatorios`) que use una carpeta `references/` con **varias plantillas** y cargue solo la que aplica a cada reporte.
2. Instalar dos skills de la comunidad, `caveman` y `markitdown`, entendiendo qué hacen y qué riesgos implica instalar skills de terceros.
3. Saber decidir cuándo una skill amerita references y cuándo no.

De nuevo, es a propósito **otra tarea distinta**: en el módulo 5 la skill decidía riesgo crediticio; aquí genera reportes regulatorios. Y el patrón de references se ve mejor con varias plantillas que con una: una sesión pide un reporte, así que solo debe cargarse esa plantilla, no las tres.

## El patrón que no viste en el módulo 5: references

En `scoring-crediticio` la skill empacaba un **script** (`validar-scoring.csx`). Los scripts son un tipo de recurso, pero hay otro igual de importante: **documentos de referencia** que Copilot lee solo cuando la tarea lo exige.

La estructura típica:

```
.github/skills/mi-skill/
├── SKILL.md              ← siempre disponible vía discovery
├── references/
│   ├── plantilla.md      ← se carga solo si el SKILL.md lo manda
│   └── ejemplos.md       ← se carga solo si el SKILL.md lo manda
└── scripts/
    └── validar.sh        ← se ejecuta, no se carga como texto
```

Recuerda los tres niveles de carga del módulo 5: discovery (frontmatter), instrucciones (cuerpo del SKILL.md), recursos (a demanda). La carpeta `references/` explota el tercer nivel: puedes tener 800 líneas de plantillas y ejemplos sin que cuesten un solo token hasta que la tarea los necesita.

La regla de diseño: **el SKILL.md es el índice, los references son el contenido**. Si tu SKILL.md pasa de ~150 líneas, probablemente hay contenido que debería vivir en un reference y cargarse a demanda.

Esto tiene una consecuencia directa en costo que retomamos en el [módulo 9](09-optimizacion-tokens.md): una skill mal estructurada que mete todo en el SKILL.md paga ese contexto completo cada vez que se activa.

## Paso 5.1.1: Crear la skill reportes-regulatorios

Contoso Banco entrega varios reportes regulatorios, cada uno con su formato
oficial: cartera vencida, envío mensual a buró de crédito y operaciones
inusuales de PLD. Cada formato es una plantilla larga y versionada aparte. Es el
caso perfecto para references: hay tres plantillas grandes, pero una sesión
genera **un** reporte, así que solo debe cargarse una plantilla.

**macOS / Linux**:

```bash
mkdir -p .github/skills/reportes-regulatorios/references
touch .github/skills/reportes-regulatorios/SKILL.md
touch .github/skills/reportes-regulatorios/references/plantilla-cartera-vencida.md
touch .github/skills/reportes-regulatorios/references/plantilla-buro-mensual.md
touch .github/skills/reportes-regulatorios/references/plantilla-operaciones-inusuales.md
```

**Windows (PowerShell)**:

```powershell
New-Item -ItemType Directory -Force -Path .github/skills/reportes-regulatorios/references | Out-Null
New-Item -ItemType File -Force -Path .github/skills/reportes-regulatorios/SKILL.md | Out-Null
New-Item -ItemType File -Force -Path .github/skills/reportes-regulatorios/references/plantilla-cartera-vencida.md | Out-Null
New-Item -ItemType File -Force -Path .github/skills/reportes-regulatorios/references/plantilla-buro-mensual.md | Out-Null
New-Item -ItemType File -Force -Path .github/skills/reportes-regulatorios/references/plantilla-operaciones-inusuales.md | Out-Null
```

## Paso 5.1.2: El SKILL.md como índice

El contenido completo está en [`ejemplos-finales/.github/skills/reportes-regulatorios/SKILL.md`](../ejemplos-finales/.github/skills/reportes-regulatorios/SKILL.md). Lo esencial es que el SKILL.md **no contiene ninguna plantilla**: es un índice que decide cuál cargar.

````markdown
---
name: reportes-regulatorios
description: Genera reportes regulatorios de Contoso Banco (cartera vencida, envío mensual a buró de crédito, operaciones inusuales de PLD) usando la plantilla oficial de cada tipo. Usa esta skill cuando se pida generar, armar o preparar un reporte regulatorio o de cumplimiento a partir de los datos de préstamos.
---

# Reportes regulatorios de Contoso Banco

... (cuerpo corto: índice + proceso + reglas duras) ...

## Tipos de reporte y su plantilla

Identifica qué reporte pide el usuario y carga **solo** la plantilla
correspondiente:

| Reporte pedido | Plantilla a cargar |
|----------------|--------------------|
| Cartera vencida / morosidad | references/plantilla-cartera-vencida.md |
| Envío mensual a buró de crédito | references/plantilla-buro-mensual.md |
| Operaciones inusuales / PLD | references/plantilla-operaciones-inusuales.md |

Si el usuario no dice cuál, pregúntale antes de cargar cualquier plantilla. No
cargues las tres "por si acaso": eso derrota el propósito de la carga progresiva.
````

Fíjate en la diferencia con una skill mal diseñada: si las tres plantillas
estuvieran pegadas en el SKILL.md, cada activación pagaría las tres. Con la tabla
de arriba, Copilot lee el índice (barato) y solo después carga la plantilla que
necesita como una lectura de archivo aparte.

## Paso 5.1.3: Los references

Las tres plantillas están en [`ejemplos-finales/.github/skills/reportes-regulatorios/references/`](../ejemplos-finales/.github/skills/reportes-regulatorios/references/). Cópialas o reconstrúyelas guiado por Copilot. Cada una define un formato regulatorio distinto:

- `plantilla-cartera-vencida.md`: resumen ejecutivo, detalle de préstamos con atraso, aging de saldos y provisiones.
- `plantilla-buro-mensual.md`: lote de registros de crédito con claves de situación de pago para el buró.
- `plantilla-operaciones-inusuales.md`: reglas de alerta PLD (montos altos, fraccionamiento, prepago) y operaciones marcadas.

Un detalle de diseño que vale la pena copiar: cada plantilla exige una sección
de **Trazabilidad** y obliga a marcar los datos que el modelo del proyecto no
tiene con `[PENDIENTE: fuente de dato]` en lugar de inventarlos. Un reporte
regulatorio con cifras inventadas es un problema legal, no un placeholder.

## Paso 5.1.4: Probar la carga progresiva

Abre una sesión nueva de chat y pide:

```
Genera el reporte de cartera vencida del periodo 2026-06
```

Verifica en el log del chat que Copilot cargó la skill y **después** leyó
`plantilla-cartera-vencida.md` como una lectura de archivo separada, sin tocar
las otras dos plantillas. Ese es el patrón funcionando: el frontmatter costó casi
nada, el cuerpo (el índice) se cargó al activarse, y solo la plantilla del
reporte pedido se leyó.

Ahora, en la misma sesión, pide un reporte distinto:

```
Ahora genera el reporte de operaciones inusuales del mismo periodo
```

Debería cargar `plantilla-operaciones-inusuales.md` y **no** volver a leer la de
cartera vencida. Cada plantilla se paga solo cuando su reporte se pide. Si Copilot
carga plantillas que no correspondían al reporte, el índice del SKILL.md no está
lo bastante claro; ajústalo.

## Skills de la comunidad

Las skills son un estándar abierto y existe un ecosistema creciente de skills publicadas por terceros. Dos que vale la pena conocer, cada una por una razón distinta:

### caveman: compresión de salida

[caveman](https://github.com/JuliusBrussee/caveman) es una skill viral que instruye al modelo a responder en modo ultra comprimido: sin artículos, sin relleno, sin cortesías, manteniendo la sustancia técnica. El repo reporta reducciones medidas de alrededor del 65% en tokens de salida. Tiene niveles de intensidad y reglas sensatas: el código, los nombres de funciones y los mensajes de error nunca se abrevian, y el modo se suspende para advertencias de seguridad y confirmaciones de acciones irreversibles.

¿Es en serio o es un chiste? Las dos cosas. El tono es humorístico, pero el mecanismo es real y medible, y en el [módulo 9](09-optimizacion-tokens.md) analizamos con números cuándo aporta y cuándo no (adelanto: solo comprime la salida, y en sesiones agénticas el costo suele estar dominado por la entrada).

### markitdown: contexto en formato eficiente

[MarkItDown](https://github.com/microsoft/markitdown) es una herramienta de Microsoft (Python, licencia MIT) que convierte PDF, Word, PowerPoint, Excel, HTML y otros formatos a Markdown. Existen skills de comunidad que la envuelven para que el agente convierta documentos por su cuenta. El caso de uso en un banco es constante: te llega la especificación regulatoria en PDF o el contrato de la API del core bancario en Word, y quieres que Copilot trabaje con ese contenido.

Markdown es el formato más eficiente en tokens para darle documentos a un modelo, y la conversión con un script es determinística (a diferencia de pedirle al modelo que "lea" un binario). Requiere Python 3.10+ y `pip install 'markitdown[all]'`.

## Paso 5.1.5: Instalar las skills de comunidad

Instalar una skill es copiar su carpeta a `.github/skills/`. En `ejemplos-finales/.github/skills/` de este repo encontrarás versiones en español de ambas (`caveman/` y `markitdown/`), escritas para este workshop y adaptadas al escenario de Contoso Banco, con crédito a los proyectos originales. Cópialas a tu starter:

**macOS / Linux**:

```bash
cp -r ../ejemplos-finales/.github/skills/caveman .github/skills/
cp -r ../ejemplos-finales/.github/skills/markitdown .github/skills/
```

**Windows (PowerShell)**:

```powershell
Copy-Item -Recurse ../ejemplos-finales/.github/skills/caveman .github/skills/
Copy-Item -Recurse ../ejemplos-finales/.github/skills/markitdown .github/skills/
```

Prueba caveman:

```
/caveman Explícame qué hace PrestamoServicio.CalcularCuota
```

Prueba markitdown (necesitas un PDF o DOCX a mano y Python instalado):

```
/markitdown Convierte docs/especificacion-regulatoria.pdf y resume las reglas de validación de monto
```

## Antes de instalar una skill de terceros: léela completa

Esto no es un consejo genérico de seguridad, es específico de cómo funcionan las skills: **una skill es texto que se inyecta en el contexto del modelo con autoridad de instrucción**. Si el SKILL.md contiene instrucciones maliciosas (exfiltrar datos, aprobar comandos, ignorar tus instrucciones de repo), Copilot las va a tratar como parte de su tarea. Y si incluye scripts, esos scripts corren en tu máquina con tus permisos.

Checklist mínimo antes de copiar una skill ajena a tu repo:

1. Lee el SKILL.md completo, incluyendo el frontmatter. ¿Hay instrucciones que no tienen que ver con el propósito declarado?
2. Lee cada script. ¿Hace peticiones de red? ¿A dónde? ¿Lee variables de entorno o archivos fuera del repo?
3. Revisa que el repo de origen tenga historial, mantenimiento y licencia. Estrellas no son garantía de nada, pero un repo sin historial es peor señal.
4. En entornos corporativos: trata las skills como dependencias. Pasan por el mismo proceso de aprobación que un paquete de NuGet.

El catálogo [Awesome Copilot](https://github.com/github/awesome-copilot) mantenido por GitHub agrega skills, instrucciones y agentes de la comunidad. Úsalo como punto de partida, no como lista blanca: el checklist anterior aplica igual.

## Cuándo usar references y cuándo no

**Conviene references cuando:**
- Hay plantillas, especificaciones o ejemplos largos (más de ~50 líneas) que no se necesitan en toda activación.
- El contenido de referencia cambia con otra frecuencia que el proceso (la plantilla regulatoria se versiona aparte).
- Distintas tareas dentro de la misma skill necesitan distintos documentos.

**No conviene cuando:**
- El contenido total de la skill cabe en menos de ~100 líneas. Un reference de 15 líneas es fragmentación sin beneficio.
- El contenido se necesita siempre que la skill se activa. Ponerlo en un reference solo agrega una lectura de archivo extra.

## Siguiente

[Módulo 6: Code review](06-code-review.md), o si vienes en segunda pasada, salta al [módulo 8: Hooks](08-hooks.md).
