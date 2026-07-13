---
name: reportes-regulatorios
description: Genera reportes regulatorios de Contoso Banco (cartera vencida, envío mensual a buró de crédito, operaciones inusuales de PLD) usando la plantilla oficial de cada tipo. Usa esta skill cuando se pida generar, armar o preparar un reporte regulatorio o de cumplimiento a partir de los datos de préstamos.
---

# Reportes regulatorios de Contoso Banco

Contoso Banco debe entregar varios reportes regulatorios con formatos oficiales
distintos. Cada tipo de reporte tiene su propia plantilla obligatoria, larga y
versionada por separado. Esta skill es el **índice**: decide qué plantilla
cargar y solo carga esa.

El punto pedagógico: hay tres plantillas grandes en `references/`, pero una
sesión típica genera **un** reporte. Cargar las tres siempre sería pagar
contexto que no se usa. Por eso el SKILL.md es corto y las plantillas se cargan
a demanda, solo la que aplica.

## Tipos de reporte y su plantilla

Identifica qué reporte pide el usuario y carga **solo** la plantilla
correspondiente:

| Reporte pedido | Plantilla a cargar |
|----------------|--------------------|
| Cartera vencida / morosidad / préstamos atrasados | [references/plantilla-cartera-vencida.md](./references/plantilla-cartera-vencida.md) |
| Envío mensual a buró de crédito | [references/plantilla-buro-mensual.md](./references/plantilla-buro-mensual.md) |
| Operaciones inusuales / PLD / prevención de lavado | [references/plantilla-operaciones-inusuales.md](./references/plantilla-operaciones-inusuales.md) |

Si el usuario no dice cuál, pregúntale antes de cargar cualquier plantilla. No
cargues las tres "por si acaso": eso derrota el propósito de la carga progresiva.

## Proceso

1. Determina el tipo de reporte a partir de la petición del usuario.
2. Carga **únicamente** la plantilla de la tabla anterior. No inventes secciones
   ni cambies el orden: la plantilla es un requisito regulatorio, no una
   sugerencia.
3. Toma los datos de los modelos y del servicio del proyecto
   (`Models/`, `Services/PrestamoServicio.cs`). Usa los préstamos semilla que
   ya existen si el usuario no aporta datos.
4. Genera el reporte en `docs/reportes/<tipo>-<periodo>.md`
   (por ejemplo `docs/reportes/cartera-vencida-2026-06.md`).

## Reglas duras

- **Nunca inventes cifras.** Si un dato que la plantilla exige no está en el
  código ni lo dio el usuario, escribe `[PENDIENTE: fuente de dato]` en esa
  celda. Un reporte regulatorio con cifras inventadas es un problema legal, no
  un placeholder cosmético.
- No inventes campos que no existen en los modelos de `Models/`.
- Los montos van en MXN con dos decimales. Los periodos en formato `AAAA-MM`.
- Cada reporte cierra con una sección **Trazabilidad**: de qué archivo y método
  salió cada bloque de datos. Es lo que hace auditable el reporte.
- Si detectas que el código no captura un dato que el regulador exige (por
  ejemplo, no hay campo de fecha de último pago para calcular días de atraso),
  repórtalo como hallazgo, no lo simules.
