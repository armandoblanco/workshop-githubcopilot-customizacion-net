---
description: Revisa código relacionado con cálculo o lógica de préstamos
agent: agent
argument-hint: "<archivo o función específica a revisar>"
model: Claude Opus 4.5 (copilot)
---

# Revisión específica de lógica de préstamos

Revisa el archivo o función indicada y aplica esta checklist en orden.
Reporta cada hallazgo con: severidad (alta, media, baja), archivo y línea,
descripción del problema, y propuesta de corrección.

## Reglas de dominio (severidad alta si fallan)

1. Todo monto se maneja con `decimal`. Si encuentras `double` o `float` para
   dinero, es severidad alta.

2. Las tasas se almacenan como decimal anual. Si ves operaciones que parecen
   tratar 18 como 18% en lugar de 0.18, es severidad alta.

3. La fórmula de amortización francesa correcta es:
   `cuota = monto * (i * (1+i)^n) / ((1+i)^n - 1)`
   donde `i` es la tasa mensual (tasa anual / 12) y `n` es el plazo en meses.
   Cualquier desviación es severidad alta.

4. Validar rangos antes de calcular: monto entre 1000 y 5000000, plazo entre
   6 y 60 meses. Si el cálculo se ejecuta sin validar, severidad alta.

## Reglas de implementación (severidad media)

5. División entre cero protegida. Si la tasa es 0, la fórmula falla.

6. Redondeo explícito al final del cálculo, no en pasos intermedios. Usar
   `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.

7. Operaciones financieras intermedias en `decimal`, no convertir a `double`
   en ningún punto.

## Reglas de estilo y mantenibilidad (severidad baja)

8. Nombres en español según las convenciones del repo.

9. Métodos públicos con comentario XML que describan parámetros, valor de
   retorno y excepciones esperadas.

10. Tests cubriendo: tasa cero, plazo mínimo, plazo máximo, monto mínimo,
    monto máximo, monto fuera de rango.

## Formato del reporte

Devuelve una tabla en Markdown con columnas: Severidad, Archivo:línea,
Hallazgo, Propuesta. Al final, un resumen de cuántos hallazgos hay por
severidad.

Si no hay hallazgos en alguna categoría, dilo explícitamente. No inventes
problemas para llenar el reporte.
