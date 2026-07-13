# Deniega comandos peligrosos antes de que el agente los ejecute.
# Política CB-SEC-014 de Contoso Banco (escenario del workshop).
# Recibe el JSON del evento por stdin; responde JSON por stdout.

$raw = [Console]::In.ReadToEnd()
try {
    $evento = $raw | ConvertFrom-Json
} catch {
    exit 0
}

if ($evento.toolName -ne "bash" -and $evento.toolName -ne "powershell") {
    exit 0
}

$comando = ""
try {
    $args_json = $evento.toolArgs | ConvertFrom-Json
    $comando = [string]$args_json.command
} catch {
    exit 0
}

$patrones = @(
    'rm -rf /',
    'rm -rf \.',
    'drop database',
    'drop table',
    'database drop',
    'curl .*\|\s*(bash|sh)',
    'wget .*\|\s*(bash|sh)',
    'iex\s*\(',
    'git push --force'
)

foreach ($p in $patrones) {
    if ($comando -imatch $p) {
        $respuesta = @{
            behavior = "deny"
            message  = "Comando bloqueado por política de Contoso Banco (CB-SEC-014). Si es legítimo, ejecútalo manualmente fuera del agente."
        }
        $respuesta | ConvertTo-Json -Compress
        exit 0
    }
}

exit 0
