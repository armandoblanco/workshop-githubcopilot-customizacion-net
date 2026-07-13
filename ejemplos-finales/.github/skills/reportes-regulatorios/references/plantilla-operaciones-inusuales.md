# Plantilla: Reporte de operaciones inusuales (formato CB-REG-PLD)

Reporte de Prevención de Lavado de Dinero (PLD). Lista operaciones que disparan
una alerta según los umbrales internos. Usa exactamente estas secciones, en este
orden.

## 1. Encabezado

- Institución: Contoso Banco S.A.
- Tipo de reporte: Operaciones inusuales (PLD).
- Periodo: `AAAA-MM`.
- Oficial de cumplimiento responsable: [nombre].

## 2. Umbrales de alerta aplicados

Reglas internas CB-REG-PLD que marcan una operación como inusual:

| Regla | Condición | Justificación |
|-------|-----------|---------------|
| PLD-1 | Préstamo por monto > 1,000,000 MXN | Operación de monto elevado |
| PLD-2 | Cliente con ≥ 3 préstamos activos simultáneos | Posible fraccionamiento |
| PLD-3 | Préstamo solicitado y liquidado en < 30 días | Patrón de lavado por prepago |

## 3. Operaciones marcadas

Una fila por operación que dispara al menos una regla:

| Id préstamo | Cliente | Monto | Regla(s) disparada(s) | Nivel de alerta |
|-------------|---------|-------|-----------------------|-----------------|

Nivel de alerta: **Alto** si dispara PLD-1 o dos o más reglas; **Medio** si
dispara una sola de PLD-2 o PLD-3.

## 4. Resumen de alertas

| Nivel | Número de operaciones |
|-------|-----------------------|
| Alto | |
| Medio | |
| Total | |

## 5. Acciones recomendadas

Por cada operación de nivel Alto, indica la acción de cumplimiento sugerida
(revisión reforzada, solicitud de documentación de origen de recursos, o reporte
a la autoridad). No decides tú la acción legal: la propones para revisión del
oficial de cumplimiento.

## 6. Trazabilidad

De qué archivo y método salió cada operación evaluada. Si una regla no se puede
evaluar con los datos actuales (por ejemplo, PLD-3 requiere fecha de liquidación
que el modelo no captura), decláralo aquí y marca esa regla como no evaluable en
lugar de asumir que ninguna operación la dispara.
