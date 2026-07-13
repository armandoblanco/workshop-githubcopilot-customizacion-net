#!/bin/bash
# Deniega comandos peligrosos antes de que el agente los ejecute.
# Política CB-SEC-014 de Contoso Banco (escenario del workshop).
# Recibe el JSON del evento por stdin; responde JSON por stdout.

INPUT=$(cat)
TOOL=$(echo "$INPUT" | jq -r '.toolName // empty')

# Solo nos interesan ejecuciones de shell
if [ "$TOOL" != "bash" ] && [ "$TOOL" != "powershell" ]; then
  exit 0
fi

COMANDO=$(echo "$INPUT" | jq -r '.toolArgs // "{}" | fromjson | .command // empty' 2>/dev/null)

PATRONES='rm -rf /|rm -rf \.|drop database|drop table|database drop|curl .*\| *(bash|sh)|wget .*\| *(bash|sh)|iex *\(|git push --force'

if echo "$COMANDO" | grep -qiE "$PATRONES"; then
  echo '{"behavior": "deny", "message": "Comando bloqueado por política de Contoso Banco (CB-SEC-014). Si es legítimo, ejecútalo manualmente fuera del agente."}'
  exit 0
fi

exit 0
