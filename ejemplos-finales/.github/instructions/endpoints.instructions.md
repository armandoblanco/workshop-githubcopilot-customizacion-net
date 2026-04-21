---
applyTo: "**/Endpoints/**/*.cs,**/Program.cs"
---

# Convenciones para endpoints HTTP

- Cada grupo de endpoints va en su propio archivo dentro de `Endpoints/`, expuesto como un método de extensión sobre `IEndpointRouteBuilder`.
- El método de extensión se llama `MapPrestamosEndpoints`, `MapClientesEndpoints`, etc.
- Cada endpoint declara `.WithName()`, `.WithTags()`, `.WithOpenApi()` y `.Produces<T>(StatusCodes.Status*)` para todas las respuestas posibles.
- Los códigos de respuesta esperados son: 200 lectura, 201 creación con header Location, 204 eliminación, 400 validación, 404 no encontrado, 422 regla de negocio violada.
- La validación de entrada se hace en el endpoint, no en el servicio. El servicio asume entradas válidas.
- Para errores de regla de negocio (ejemplo: monto fuera de rango) se devuelve `Results.UnprocessableEntity` con un objeto `{ codigo, mensaje }` en español.
