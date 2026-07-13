# Plantilla: Reporte de cartera vencida (formato CB-REG-R04)

Reporte de préstamos con atraso, alineado al reporte regulatorio R04 de cartera
crediticia. Usa exactamente estas secciones, en este orden.

## 1. Encabezado

- Institución: Contoso Banco S.A.
- Tipo de reporte: Cartera vencida (R04).
- Periodo: `AAAA-MM`.
- Fecha de corte: último día del periodo.
- Responsable de generación: [usuario o sistema].

## 2. Resumen ejecutivo

Tabla de una fila con los totales de la cartera al corte:

| Métrica | Valor |
|---------|-------|
| Total de préstamos activos | entero |
| Préstamos en cartera vencida (atraso > 90 días) | entero |
| Saldo total de la cartera | MXN |
| Saldo en cartera vencida | MXN |
| Índice de morosidad (saldo vencido / saldo total) | % con dos decimales |

## 3. Detalle de préstamos vencidos

Una fila por préstamo con atraso > 90 días:

| Id préstamo | Cliente | Saldo insoluto | Días de atraso | Banda de riesgo | Estado |
|-------------|---------|----------------|----------------|-----------------|--------|

Ordena de mayor a menor saldo insoluto.

## 4. Clasificación por antigüedad de saldos (aging)

Distribución del saldo vencido por rango de atraso:

| Rango de atraso | Número de préstamos | Saldo | % del saldo vencido |
|-----------------|---------------------|-------|---------------------|
| 91–180 días | | | |
| 181–365 días | | | |
| > 365 días | | | |

## 5. Provisiones estimadas

Provisión sugerida por rango de aging (regla interna CB-REG-R04):

- 91–180 días: 25% del saldo.
- 181–365 días: 60% del saldo.
- > 365 días: 100% del saldo.

Reporta el monto de provisión por rango y el total.

## 6. Trazabilidad

De qué archivo y método salió cada bloque de datos (por ejemplo:
`Services/PrestamoServicio.cs → ObtenerTodos()`). Si un dato no existe en el
código (por ejemplo, "días de atraso" requiere fecha de último pago que el
modelo no tiene), decláralo aquí como dato faltante y marca la celda
correspondiente con `[PENDIENTE: fuente de dato]`.
