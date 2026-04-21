---
description: Revisa los endpoints HTTP del repo enfocándose en seguridad y validación
agent: agent
model: Claude Opus 4.5 (copilot)
---

# Revisión enfocada de endpoints

Lee todos los archivos en `Endpoints/` y `Program.cs`. Para cada endpoint
encontrado, evalúa:

1. **Validación de entrada**: el endpoint valida tipos, rangos, formato antes
   de invocar al servicio?

2. **Códigos de respuesta**: el endpoint declara `Produces<T>` para todos los
   códigos posibles? Se devuelven los códigos correctos (200, 201 con
   Location, 204, 400, 404, 422)?

3. **Información en respuestas de error**: los mensajes de error son
   informativos sin filtrar detalles internos del sistema (stacktraces,
   nombres de tablas, paths de archivos)?

4. **Idempotencia**: los métodos GET son realmente idempotentes? Los DELETE
   manejan correctamente el caso de no encontrado?

5. **Autorización**: hay endpoints que deberían requerir autorización y no
   la requieren? (En este proyecto educativo no hay auth implementada, así
   que solo señala dónde habría que agregarla en producción).

## Formato

Tabla por endpoint con columnas: Endpoint, Verbo+Ruta, Severidad del problema
más alto encontrado, Resumen. Después, una sección por endpoint con detalle
de hallazgos.

No reportes endpoints que están bien. Solo los que tienen problemas.
