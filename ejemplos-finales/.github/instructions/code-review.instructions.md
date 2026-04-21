---
applyTo: "**/*.cs"
excludeAgent: ['vscode']
---

# Reglas específicas para code review automatizado

Esta lista se aplica cuando GitHub Copilot revisa pull requests. Cada hallazgo
debe incluir una propuesta concreta de corrección.

## Severidad alta (debe bloquear el merge)

- Uso de `double` o `float` para representar dinero. Sustituir por `decimal`.
- Fórmulas financieras sin validación de entrada (rangos de monto, plazo, tasa).
- Información sensible en código fuente: claves, conexiones, datos personales.
- División entre cero sin guardia.
- Excepciones genéricas (`catch (Exception)` sin re-lanzar) que ocultan errores.

## Severidad media (debería resolverse antes del merge)

- Métodos públicos sin comentarios XML.
- Tests faltantes para casos borde (monto mínimo, plazo máximo, tasa cero).
- Nombres en inglés en archivos donde el resto está en español.
- Magic numbers en código de cálculo (debe haber constantes nombradas).

## Severidad baja (nota informativa)

- Imports no usados.
- Espacios en blanco inconsistentes.
- Comentarios desactualizados.

## Lo que NO debes reportar

- Sugerencias de cambiar el estilo de Minimal API a Controllers (ya es decisión arquitectónica).
- Sugerencias de agregar Entity Framework o cambiar el almacenamiento.
- Sugerencias de logging o telemetría a menos que sean para errores no manejados.
- Sugerencias de agregar try/catch defensivamente sin causa específica.
