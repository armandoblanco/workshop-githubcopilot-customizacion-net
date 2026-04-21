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
dotnet script .github/skills/calculo-prestamo/validar.csx
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
