# Módulo 5: Agent Skills

Tiempo estimado: 20 minutos.

## Objetivo

Crear una skill que enseñe a Copilot a calcular cuotas de préstamo aplicando la metodología correcta del banco. La skill incluye un script de validación que Copilot puede correr para verificar sus propios cálculos.

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

La consecuencia práctica: el `description` debe ser específico. Si dice "ayuda con cálculos", Copilot no va a saber cuándo cargarla. Si dice "calcula cuotas de préstamo, valida amortización francesa, genera cronogramas de pago", la activación se vuelve confiable.

## Paso 5.1: Crear la estructura

Crea la carpeta y los archivos:

```bash
mkdir -p .github/skills/calculo-prestamo/ejemplos
touch .github/skills/calculo-prestamo/SKILL.md
touch .github/skills/calculo-prestamo/validar.csx
touch .github/skills/calculo-prestamo/ejemplos/cronograma-ejemplo.json
```

Importante: el nombre de la carpeta debe coincidir exactamente con el campo `name` del frontmatter. Si la carpeta es `calculo-prestamo`, `name` debe ser `calculo-prestamo`. Si no coinciden, la skill no se carga.

## Paso 5.2: Escribir el SKILL.md

Contenido de `.github/skills/calculo-prestamo/SKILL.md`:

```markdown
---
name: calculo-prestamo
description: Calcula cuotas mensuales, costo total y cronogramas de pago para préstamos de Contoso Banco usando amortización francesa. Usa esta skill cuando se pida calcular, simular o explicar cálculos de préstamo.
---

# Cálculo de préstamos de Contoso Banco

Esta skill encapsula la metodología de cálculo de préstamos de Contoso Banco.
Úsala cuando el usuario pida cualquiera de estas cosas:

- Simular un préstamo (calcular la cuota mensual antes de crearlo).
- Generar el cronograma de pagos.
- Explicar por qué un cálculo da cierto resultado.
- Validar que un código de cálculo es correcto.

## Fórmula de amortización francesa

La cuota mensual constante se calcula así:

```
cuota = monto * (i * (1 + i)^n) / ((1 + i)^n - 1)
```

Donde:
- `monto` es el capital prestado.
- `i` es la tasa de interés mensual (tasa anual / 12).
- `n` es el plazo en meses.

Caso especial: si la tasa es 0, la cuota es `monto / n`. La fórmula anterior
da división entre cero.

## Generación del cronograma

Cada fila del cronograma tiene:

- Número de cuota (1, 2, ..., n).
- Cuota total (constante).
- Interés del periodo: `saldo_inicial * i`.
- Capital del periodo: `cuota - interés`.
- Saldo final: `saldo_inicial - capital`.

El primer saldo inicial es el monto del préstamo. La última cuota debe dejar
el saldo en exactamente 0. Si por redondeos no queda en cero, ajusta la
última cuota al saldo restante.

## Reglas operativas del banco

- Monto válido: entre 1000 MXN y 5000000 MXN.
- Plazo válido: entre 6 y 60 meses.
- Tasa siempre se almacena como decimal anual (0.18 representa 18%).
- Todos los cálculos en `decimal`. No usar `double` en ningún paso intermedio.
- Redondeo a dos decimales únicamente al mostrar al usuario, no en cálculos.

## Validación de tu propio trabajo

Después de generar código de cálculo, ejecuta el script
[validar.csx](./validar.csx) con tres casos de prueba para confirmar que tu
implementación da los mismos resultados que los esperados:

```
dotnet script .github/skills/calculo-prestamo/validar.csx -- <ruta-a-tu-implementacion>
```

Si los resultados no coinciden con la salida esperada (ver el archivo de
ejemplo [cronograma-ejemplo.json](./ejemplos/cronograma-ejemplo.json)), tu
implementación tiene un bug. Reporta el bug al usuario antes de declarar la
tarea completa.

## Casos de prueba estándar

| Caso | Monto | Tasa anual | Plazo | Cuota esperada |
|------|-------|------------|-------|----------------|
| 1 | 100000 | 0.18 | 12 | 9168.00 |
| 2 | 50000 | 0.12 | 24 | 2353.67 |
| 3 | 200000 | 0.00 | 36 | 5555.56 |

Valores redondeados a dos decimales con `MidpointRounding.AwayFromZero`. Si
usas `ToEven` (banker's rounding) los resultados pueden diferir en 0.01.

Si tu implementación no produce estos valores con dos decimales de
precisión, está mal. No los aceptes con "casi correcto".
```

## Paso 5.3: Crear el script de validación

Contenido de `.github/skills/calculo-prestamo/validar.csx`:

```csharp
#!/usr/bin/env dotnet-script
// Script de validación de cálculo de cuotas.
// Uso: dotnet script validar.csx -- <ruta-al-csproj-de-la-implementacion>
// Compara contra los casos de prueba estándar de Contoso Banco.

decimal CalcularCuotaEsperada(decimal monto, decimal tasaAnual, int plazoMeses)
{
    if (tasaAnual == 0m)
        return Math.Round(monto / plazoMeses, 2, MidpointRounding.AwayFromZero);

    decimal i = tasaAnual / 12m;
    decimal factor = 1m;
    for (int k = 0; k < plazoMeses; k++)
        factor *= (1m + i);

    decimal cuota = monto * (i * factor) / (factor - 1m);
    return Math.Round(cuota, 2, MidpointRounding.AwayFromZero);
}

var casos = new (decimal monto, decimal tasa, int plazo, decimal esperado)[]
{
    (100000m, 0.18m, 12, 9168.00m),
    (50000m,  0.12m, 24, 2353.67m),
    (200000m, 0.00m, 36, 5555.56m)
};

bool todosPasan = true;
foreach (var caso in casos)
{
    var calculada = CalcularCuotaEsperada(caso.monto, caso.tasa, caso.plazo);

    var diferencia = Math.Abs(calculada - caso.esperado);
    var paso = diferencia <= 0.01m;

    Console.WriteLine(
        $"Caso monto={caso.monto} tasa={caso.tasa} plazo={caso.plazo}: " +
        $"esperado {caso.esperado}, calculado {calculada} - " +
        (paso ? "OK" : "FALLA"));

    if (!paso) todosPasan = false;
}

Environment.Exit(todosPasan ? 0 : 1);
```

Si los participantes no tienen `dotnet-script` instalado, pueden instalarlo con `dotnet tool install -g dotnet-script`. Alternativa: el script lo puede ejecutar Copilot directamente desde el chat usando la tool de comandos.

## Paso 5.4: Crear el ejemplo de cronograma

Contenido de `.github/skills/calculo-prestamo/ejemplos/cronograma-ejemplo.json`:

```json
{
  "descripcion": "Ejemplo de salida esperada para monto=100000 MXN, tasa=18% anual, plazo=12 meses.",
  "entrada": {
    "monto": 100000,
    "tasaAnual": 0.18,
    "plazoMeses": 12
  },
  "resultado": {
    "cuotaMensual": 9168.00,
    "costoTotalAjustado": 110015.99,
    "interesesTotales": 10015.99
  },
  "primerasFilas": [
    { "numero": 1, "cuota": 9168.00, "interes": 1500.00, "capital": 7668.00, "saldoFinal": 92332.00 },
    { "numero": 2, "cuota": 9168.00, "interes": 1384.98, "capital": 7783.02, "saldoFinal": 84548.98 },
    { "numero": 3, "cuota": 9168.00, "interes": 1268.23, "capital": 7899.77, "saldoFinal": 76649.21 }
  ],
  "ultimaFila": {
    "numero": 12, "cuota": 9167.99, "interes": 135.49, "capital": 9032.50, "saldoFinal": 0.00
  }
}
```

## Paso 5.5: Verificar que la skill aparece

1. Abre el editor de Chat Customizations (`Chat: Open Chat Customizations`).
2. Ve a la pestaña **Skills**.
3. Debería aparecer `calculo-prestamo` con un check verde.

Si no aparece, las causas comunes son:

- El nombre de la carpeta no coincide con el campo `name` del frontmatter.
- El frontmatter YAML está mal formado.
- El archivo no se llama exactamente `SKILL.md` (cuidado con mayúsculas).

## Paso 5.6: Probar la skill

Hay dos maneras de invocar una skill: explícitamente con `/` o dejando que Copilot la cargue automáticamente cuando determina que es relevante.

**Invocación explícita:**

```
/calculo-prestamo Genera el método CalcularCuota en PrestamoServicio.cs y valídalo con los casos de prueba estándar
```

Verifica que:

1. Copilot genera el método usando la fórmula correcta.
2. Detecta el caso especial de tasa 0.
3. Usa `decimal` en todos lados.
4. Ejecuta el script de validación al final.
5. Reporta los resultados de los tres casos de prueba.

**Invocación automática:**

```
Necesito agregar al endpoint POST /api/prestamos/simular la generación del cronograma de pagos completo
```

Si la skill está bien escrita, Copilot debería decidir cargarla por su cuenta basándose en el `description`. Verás en el log del chat algo como "Loaded skill: calculo-prestamo".

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

[Módulo 6: Code review](06-code-review.md)
