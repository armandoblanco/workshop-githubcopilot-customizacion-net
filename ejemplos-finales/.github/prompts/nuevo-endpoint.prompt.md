---
description: Genera un endpoint Minimal API completo para Contoso Banco
agent: agent
argument-hint: "<descripción del endpoint en lenguaje natural>"
---

# Generar nuevo endpoint

Vas a crear un endpoint nuevo en el proyecto Contoso Banco - Servicio de Préstamos.

## Lo que recibes

El usuario describe el endpoint en lenguaje natural. Por ejemplo:
"un GET que devuelve los préstamos de un cliente filtrados por estado".

## Pasos a seguir

1. Determina el grupo al que pertenece el endpoint (préstamos, clientes, etc.).
   Si no existe el archivo `Endpoints/<Grupo>Endpoints.cs`, créalo siguiendo
   el patrón de los archivos existentes en `Endpoints/`.

2. Define la ruta y verbo HTTP siguiendo convenciones REST.

3. Define los DTOs de entrada y salida como `record` en `Models/`. Reutiliza
   los existentes si ya hay uno apropiado.

4. Implementa el endpoint usando `MapGet`, `MapPost`, `MapPut` o `MapDelete`
   según corresponda.

5. Aplica todas las convenciones definidas en
   `.github/instructions/endpoints.instructions.md`:
   - `.WithName()`, `.WithTags()`, `.WithOpenApi()` siempre.
   - `.Produces<T>()` para cada código de respuesta posible.
   - Validación en el endpoint, no en el servicio.
   - 422 para errores de regla de negocio.

6. Si el endpoint requiere lógica nueva en algún servicio, agrégala en
   `Services/`. No la metas inline en el endpoint.

7. Genera al menos un test de integración en
   `ContosoBanco.Loans.Tests/Endpoints/` que cubra el camino feliz y un error
   de validación.

## Lo que NO debes hacer

- No agregar paquetes nuevos.
- No introducir Controllers.
- No crear documentación nueva fuera de los comentarios XML.
- No modificar endpoints existentes.

## Formato de salida

Antes de crear nada, muestra al usuario:
- Ruta y verbo HTTP propuestos.
- DTOs nuevos que vas a crear.
- Servicios que vas a tocar.
- Tests que vas a generar.

Espera confirmación. Si el usuario dice "adelante", procede a crear los archivos.
