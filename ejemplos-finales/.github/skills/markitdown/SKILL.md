---
name: markitdown
description: Convierte documentos (PDF, DOCX, PPTX, XLSX, HTML, CSV) a Markdown usando la herramienta MarkItDown de Microsoft, para incorporarlos como contexto eficiente en tokens. Usa esta skill cuando se pida convertir, extraer o leer el contenido de un documento de oficina o PDF.
---

# Conversión de documentos a Markdown

Envuelve [MarkItDown](https://github.com/microsoft/markitdown), la
herramienta de Microsoft (Python, licencia MIT) para convertir formatos
de documento a Markdown. Markdown es el formato más eficiente en tokens
para dar documentos a un modelo, y la conversión con script es
determinística: no dependes de que el modelo "interprete" un binario.

## Pre-requisito

Python 3.10+ y el paquete instalado:

```
pip install 'markitdown[all]'
```

Si el comando `markitdown` no existe, reporta al usuario el comando de
instalación anterior en lugar de intentar leer el binario directamente.

## Proceso

1. Verifica que el archivo de entrada existe y su extensión está
   soportada (pdf, docx, pptx, xlsx, xls, html, csv, json, xml, epub).
2. Convierte a un archivo Markdown junto al original:

   ```
   markitdown <archivo-entrada> -o <archivo-entrada>.md
   ```

3. Lee el `.md` resultante, no el original, para cualquier análisis
   posterior.
4. Si el resultado supera ~500 líneas y el usuario pidió algo puntual
   (por ejemplo "las reglas de validación de monto"), extrae solo las
   secciones relevantes al contexto en lugar de cargar todo.

## Reglas

- Nunca pegues el documento completo en la respuesta si el usuario pidió
  un resumen o una extracción: responde con lo pedido y menciona la ruta
  del `.md` generado.
- Documentos escaneados (imagen dentro de PDF) pueden salir vacíos:
  MarkItDown sin extras no hace OCR confiable. Si el resultado está
  vacío o casi vacío, repórtalo en lugar de inventar contenido.
- No conviertas archivos fuera del workspace del proyecto sin
  confirmación del usuario.

## Contexto de Contoso Banco

El uso típico en este repo: especificaciones regulatorias en PDF y
contratos de API del core bancario en DOCX. Al convertirlos, guárdalos
en `docs/fuentes/` para que queden versionados junto al código que los
implementa.
