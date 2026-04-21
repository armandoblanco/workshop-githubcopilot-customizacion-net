# Starter: ContosoBanco Loans API

Este es el punto de partida del workshop. Una API mínima de préstamos con dos
endpoints funcionales, un servicio con bugs deliberados y un test de smoke.

> Los comandos `dotnet` y `git` de este archivo funcionan igual en macOS, Linux y Windows (PowerShell o cmd).

## Correr

```bash
dotnet run --project ContosoBanco.Loans.Api
```

Swagger: http://localhost:5000/swagger

## Estructura

```
ContosoBanco.Loans.Api/
├── Program.cs
├── Endpoints/
│   └── PrestamosEndpoints.cs
├── Models/
│   └── Prestamo.cs
├── Services/
│   └── PrestamoServicio.cs         <- Tiene bugs deliberados
└── Properties/
    └── launchSettings.json

ContosoBanco.Loans.Tests/
└── ApiSmokeTests.cs
```

## Bugs deliberados en `PrestamoServicio.cs`

Son los que el workshop va a usar para demostrar code review, prompt files de
revisión y el agente revisor de seguridad. No los corrijas antes de empezar:

- `CalcularCuota` usa `double` en lugar de `decimal` para manejar dinero.
- La fórmula de amortización francesa está mal (le falta multiplicar el
  numerador por `factor`).
- No hay guardia para tasa cero (división entre cero).
- No hay validación de rangos de monto, tasa ni plazo.
- El `catch (Exception)` oculta cualquier error devolviendo 0.
- Varios magic numbers (12, Math.Round sin MidpointRounding explícito).

## Ejecutar tests

```bash
dotnet test
```

Los dos smoke tests deberían pasar. Durante el workshop se agregan más tests
a medida que se arreglan bugs.
