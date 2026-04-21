#!/usr/bin/env dotnet-script
// Script de validación de cálculo de cuotas.
// Uso: dotnet script validar.csx
// Compara el cálculo contra los casos de prueba estándar de Contoso Banco.
//
// Si este script devuelve código de salida distinto de 0, la fórmula de
// amortización francesa está mal implementada en quien la está llamando.

decimal CalcularCuotaEsperada(decimal monto, decimal tasaAnual, int plazoMeses)
{
    if (tasaAnual == 0m)
        return Math.Round(monto / plazoMeses, 2, MidpointRounding.AwayFromZero);

    decimal i = tasaAnual / 12m;
    decimal factor = 1m;
    for (int k = 0; k < plazoMeses; k++)
        factor *= (1m + i);

    decimal cuota = monto * (i * factor) / (factor - 1m);
    return Math.Round(cuota, 2, MidpointRounding.AwayFromZero);
}

var casos = new (decimal monto, decimal tasa, int plazo, decimal esperado)[]
{
    (100000m, 0.18m, 12, 9168.00m),
    (50000m,  0.12m, 24, 2353.67m),
    (200000m, 0.00m, 36, 5555.56m)
};

bool todosPasan = true;
Console.WriteLine("Validando casos de prueba estándar de Contoso Banco");
Console.WriteLine("===================================================");

foreach (var caso in casos)
{
    var calculada = CalcularCuotaEsperada(caso.monto, caso.tasa, caso.plazo);
    var diferencia = Math.Abs(calculada - caso.esperado);
    var paso = diferencia <= 0.01m;

    Console.WriteLine(
        $"  monto={caso.monto,10} tasa={caso.tasa} plazo={caso.plazo,3} " +
        $"esperado={caso.esperado,10} calculado={calculada,10} " +
        (paso ? "OK" : "FALLA"));

    if (!paso) todosPasan = false;
}

Console.WriteLine("===================================================");
Console.WriteLine(todosPasan ? "Todos los casos pasan" : "Hay casos que fallan");

Environment.Exit(todosPasan ? 0 : 1);
