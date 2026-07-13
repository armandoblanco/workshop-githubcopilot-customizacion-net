# Plantilla: Envío mensual a buró de crédito (formato CB-REG-BURO)

Reporte mensual del comportamiento de pago de cada cliente que se envía al buró
de crédito. Usa exactamente estas secciones, en este orden.

## 1. Encabezado del lote

- Institución que reporta: Contoso Banco S.A. (clave de otorgante: CB-0001).
- Periodo reportado: `AAAA-MM`.
- Fecha de generación del lote.
- Número de registros en el lote.

## 2. Registros de crédito

Un registro por préstamo activo. Todos los campos son obligatorios; los que no
existan en el código se marcan `[PENDIENTE: fuente de dato]`.

| Campo | Descripción | Origen esperado |
|-------|-------------|-----------------|
| Id cliente | Identificador interno del cliente | `Prestamo.ClienteId` |
| Id crédito | Identificador del préstamo | `Prestamo.Id` |
| Tipo de crédito | Siempre "PL" (préstamo personal) | fijo |
| Monto original | Monto otorgado | `Prestamo.Monto` |
| Saldo actual | Saldo insoluto al corte | `[PENDIENTE: fuente de dato]` |
| Pago mensual | Cuota pactada | cálculo de cuota |
| Situación del pago | Ver tabla de claves abajo | derivado de historial |
| Fecha de apertura | Fecha de originación | `[PENDIENTE: fuente de dato]` |

## 3. Claves de situación del pago

Traduce el estado interno a la clave del buró:

| Estado interno | Clave buró | Significado |
|----------------|-----------|-------------|
| al corriente | 1 | Pago puntual |
| atraso 1–29 días | 2 | Atraso leve |
| atraso 30–89 días | 3 | Atraso moderado |
| atraso ≥ 90 días | 4 | Cartera vencida |
| liquidado | 0 | Crédito cerrado sin adeudo |

## 4. Totales de control

- Suma de montos originales del lote.
- Conteo de registros por clave de situación (1, 2, 3, 4, 0).

Estos totales sirven para que el buró valide la integridad del lote.

## 5. Trazabilidad

De qué archivo y método salió cada campo. Marca explícitamente qué campos
obligatorios del buró **no** existen hoy en el modelo del proyecto (saldo
actual, fecha de apertura, situación de pago real). Es un hallazgo de cumplimiento
válido, no un error tuyo.
