using ContosoBanco.Loans.Api.Endpoints;
using ContosoBanco.Loans.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new()
    {
        Title = "API de Préstamos - Contoso Banco",
        Version = "v1"
    });
});

builder.Services.AddSingleton<PrestamoServicio>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapPrestamosEndpoints();

app.Run();

public partial class Program { }
