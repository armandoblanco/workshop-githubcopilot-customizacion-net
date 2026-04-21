namespace ContosoBanco.Loans.Api.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public decimal Monto { get; set; }
    public decimal TasaAnual { get; set; }
    public int PlazoMeses { get; set; }
    public string Estado { get; set; } = "solicitado";
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
}

public record PrestamoDto(int ClienteId, decimal Monto, decimal TasaAnual, int PlazoMeses);

public record SimulacionResultado(decimal Monto, decimal TasaAnual, int PlazoMeses, decimal CuotaMensual, decimal CostoTotal);
