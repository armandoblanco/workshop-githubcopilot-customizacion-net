#!/usr/bin/env dotnet-script
// Script de validación del scoring crediticio de Contoso Banco (CB-RISK-2024).
// Uso: dotnet script validar-scoring.csx
// Compara el resultado contra los casos de prueba estándar.
//
// Si este script devuelve código de salida distinto de 0, la implementación
// del scoring está mal: normalmente porque olvida la capa 2 (reglas duras).

int PuntajePorDti(decimal dti) => dti switch
{
    <= 0.20m => 150,
    <= 0.35m => 80,
    <= 0.45m => 20,
    _        => -120
};

int PuntajePorHistorial(string historial) => historial switch
{
    "excelente" => 120,
    "bueno"     => 50,
    "regular"   => -30,
    "malo"      => -60,
    _ => throw new ArgumentException($"Historial inválido: {historial}")
};

int PuntajePorAntiguedad(int meses) => meses switch
{
    >= 36 => 80,
    >= 12 => 40,
    >= 6  => 10,
    _     => -60
};

int PuntajePorMonto(decimal relacion) => relacion switch
{
    <= 1.0m => 50,
    <= 2.0m => 10,
    _       => -70
};

(int score, string banda, string decision) Evaluar(
    decimal ingreso, decimal cuota, string historial, int antiguedad, decimal monto)
{
    decimal dti = cuota / ingreso;
    decimal relacionMonto = monto / (ingreso * 12m);

    int suma = 500
        + PuntajePorDti(dti)
        + PuntajePorHistorial(historial)
        + PuntajePorAntiguedad(antiguedad)
        + PuntajePorMonto(relacionMonto);

    int score = Math.Clamp(suma, 300, 850);

    // Capa 1: banda por score
    string banda =
        score >= 720 ? "A" :
        score >= 660 ? "B" :
        score >= 600 ? "C" : "D";

    // Capa 2: reglas duras (solo degradan)
    if (historial == "malo")
        banda = "D";                                   // RD-1
    else if (dti > 0.45m && (banda == "A" || banda == "B"))
        banda = "C";                                   // RD-2

    string decision = banda switch
    {
        "A" => "Aprobado automático",
        "B" => "Aprobado con revisión estándar",
        "C" => "Revisión manual obligatoria",
        _   => "Rechazado"
    };

    return (score, banda, decision);
}

var casos = new (decimal ingreso, decimal cuota, string historial, int antiguedad, decimal monto, int scoreEsp, string bandaEsp)[]
{
    (30000m, 6000m,  "excelente", 48, 100000m, 850, "A"),
    (20000m, 8000m,  "bueno",     18, 250000m, 620, "C"),
    (80000m, 12000m, "malo",      60, 100000m, 720, "D")
};

bool todosPasan = true;
Console.WriteLine("Validando scoring CB-RISK-2024");
Console.WriteLine("==============================");

foreach (var c in casos)
{
    var (score, banda, decision) = Evaluar(c.ingreso, c.cuota, c.historial, c.antiguedad, c.monto);
    bool paso = score == c.scoreEsp && banda == c.bandaEsp;

    Console.WriteLine(
        $"  ingreso={c.ingreso,8} historial={c.historial,-9} " +
        $"score={score,3} banda={banda} ({decision}) " +
        (paso ? "OK" : $"FALLA (esperado score={c.scoreEsp} banda={c.bandaEsp})"));

    if (!paso) todosPasan = false;
}

Console.WriteLine("==============================");
Console.WriteLine(todosPasan ? "Todos los casos pasan" : "Hay casos que fallan");

Environment.Exit(todosPasan ? 0 : 1);
