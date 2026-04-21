using ContosoBanco.Loans.Api.Models;

namespace ContosoBanco.Loans.Api.Services;

/// <summary>
/// Servicio para gestión de préstamos de Contoso Banco.
/// </summary>
/// <remarks>
/// IMPORTANTE PARA EL WORKSHOP: este archivo contiene bugs deliberados
/// que el participante debe descubrir usando code review, prompt files
/// y custom agents. No lo corrijas fuera de los ejercicios del workshop.
///
/// Problemas que deben detectarse:
/// - Uso de double en lugar de decimal en CalcularCuota.
/// - Fórmula de amortización incorrecta (le falta el factor en el numerador).
/// - Sin guardia para tasa cero (división entre cero).
/// - Sin validación de rangos antes del cálculo.
/// - Catch genérico que oculta errores.
/// - Método público sin comentario XML.
/// - Magic numbers sin nombrar (12, 0.01).
/// </remarks>
public class PrestamoServicio
{
    private readonly List<Prestamo> _prestamos = new();
    private int _siguienteId = 1;

    public PrestamoServicio()
    {
        _prestamos.Add(new Prestamo
        {
            Id = _siguienteId++,
            ClienteId = 1,
            Monto = 150000m,
            TasaAnual = 0.18m,
            PlazoMeses = 24,
            Estado = "activo"
        });

        _prestamos.Add(new Prestamo
        {
            Id = _siguienteId++,
            ClienteId = 2,
            Monto = 50000m,
            TasaAnual = 0.12m,
            PlazoMeses = 12,
            Estado = "activo"
        });
    }

    public IEnumerable<Prestamo> ObtenerTodos() => _prestamos;

    public Prestamo? ObtenerPorId(int id) => _prestamos.FirstOrDefault(p => p.Id == id);

    public Prestamo Crear(PrestamoDto dto)
    {
        var prestamo = new Prestamo
        {
            Id = _siguienteId++,
            ClienteId = dto.ClienteId,
            Monto = dto.Monto,
            TasaAnual = dto.TasaAnual,
            PlazoMeses = dto.PlazoMeses,
            Estado = "solicitado"
        };

        _prestamos.Add(prestamo);
        return prestamo;
    }

    public bool Eliminar(int id)
    {
        var prestamo = ObtenerPorId(id);
        if (prestamo is null) return false;

        _prestamos.Remove(prestamo);
        return true;
    }

    // BUG DELIBERADO: este método usa double en lugar de decimal,
    // la fórmula está incompleta, no valida rangos y no maneja tasa cero.
    public double CalcularCuota(double monto, double tasa, int plazo)
    {
        try
        {
            double i = tasa / 12;
            double factor = Math.Pow(1 + i, plazo);
            double cuota = monto * i / (factor - 1);
            return Math.Round(cuota, 2);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public SimulacionResultado Simular(decimal monto, decimal tasaAnual, int plazoMeses)
    {
        var cuotaDouble = CalcularCuota((double)monto, (double)tasaAnual, plazoMeses);
        var cuota = (decimal)cuotaDouble;
        var costoTotal = cuota * plazoMeses;

        return new SimulacionResultado(monto, tasaAnual, plazoMeses, cuota, costoTotal);
    }
}
