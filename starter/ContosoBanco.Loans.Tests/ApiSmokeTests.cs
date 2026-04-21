using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ContosoBanco.Loans.Tests;

/// <summary>
/// Tests de smoke mínimos para verificar que la API arranca.
/// El workshop agregará más tests a medida que se detecten bugs y se
/// implementen nuevas features.
/// </summary>
public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPrestamos_RetornaOk()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/prestamos");

        Assert.Equal(System.Net.HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetPrestamoInexistente_Retorna404()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/prestamos/9999");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}
