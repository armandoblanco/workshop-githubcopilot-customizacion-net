# Instrucciones generales de Contoso Banco - Servicio de Préstamos

## Idioma y comunicación

- Todo el código fuente, comentarios, documentación y mensajes de error se escriben en español.
- Excepciones permitidas en inglés: palabras técnicas estándar (Get, Post, Async, Task, Dto, Id, Api).
- Los mensajes que pueda ver el usuario final en respuestas HTTP deben ser claros y sin jerga técnica.

## Stack

- .NET 8 (LTS).
- Minimal APIs, no Controllers.
- xUnit para pruebas.
- Datos en memoria (no Entity Framework, no SQL). El proyecto es educativo.

## Convenciones de código

- Nombres en PascalCase para tipos y miembros públicos en español (CalcularCuotaMensual, MontoMaximo).
- camelCase para variables locales y parámetros (montoSolicitado, tasaInteres).
- Usa `decimal` para todo monto de dinero. Nunca `double` ni `float`.
- Usa `record` para DTOs.
- Usa `IClienteServicio` para nombres de interfaces (prefijo I).

## Reglas de negocio del dominio

- Toda tasa de interés se almacena como decimal anual (0.18m representa 18%).
- Todo monto se almacena en pesos mexicanos (MXN) sin conversión.
- El plazo se mide en meses.
- Un préstamo no puede tener monto menor a 1000 ni mayor a 5000000.
- El plazo no puede ser menor a 6 meses ni mayor a 60 meses.

## Lo que NO debe hacer Copilot

- No instales paquetes nuevos sin justificación explícita.
- No introduzcas dependencias externas (Dapper, AutoMapper, MediatR, etc) sin que se te pida.
- No agregues logging ni telemetría a menos que el prompt lo pida.
