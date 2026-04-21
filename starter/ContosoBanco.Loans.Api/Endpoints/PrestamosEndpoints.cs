using ContosoBanco.Loans.Api.Models;
using ContosoBanco.Loans.Api.Services;

namespace ContosoBanco.Loans.Api.Endpoints;

public static class PrestamosEndpoints
{
    public static IEndpointRouteBuilder MapPrestamosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/prestamos").WithTags("Prestamos");

        grupo.MapGet("/", (PrestamoServicio servicio) =>
            Results.Ok(servicio.ObtenerTodos()))
            .WithName("ObtenerPrestamos")
            .WithOpenApi()
            .Produces<IEnumerable<Prestamo>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:int}", (int id, PrestamoServicio servicio) =>
        {
            var prestamo = servicio.ObtenerPorId(id);
            return prestamo is null ? Results.NotFound() : Results.Ok(prestamo);
        })
            .WithName("ObtenerPrestamoPorId")
            .WithOpenApi()
            .Produces<Prestamo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        grupo.MapPost("/", (PrestamoDto dto, PrestamoServicio servicio) =>
        {
            var prestamo = servicio.Crear(dto);
            return Results.Created($"/api/prestamos/{prestamo.Id}", prestamo);
        })
            .WithName("CrearPrestamo")
            .WithOpenApi()
            .Produces<Prestamo>(StatusCodes.Status201Created);

        grupo.MapPost("/simular", (PrestamoDto dto, PrestamoServicio servicio) =>
        {
            var resultado = servicio.Simular(dto.Monto, dto.TasaAnual, dto.PlazoMeses);
            return Results.Ok(resultado);
        })
            .WithName("SimularPrestamo")
            .WithOpenApi()
            .Produces<SimulacionResultado>(StatusCodes.Status200OK);

        grupo.MapDelete("/{id:int}", (int id, PrestamoServicio servicio) =>
            servicio.Eliminar(id) ? Results.NoContent() : Results.NotFound())
            .WithName("EliminarPrestamo")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
