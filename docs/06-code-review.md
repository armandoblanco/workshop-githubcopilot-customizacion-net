# Módulo 6: Code review con GitHub Copilot

Tiempo estimado: 20 minutos.

## Objetivo

Configurar y usar code review en dos lugares:

1. **Dentro de VS Code**: pedir review de un fragmento o archivo seleccionado.
2. **En GitHub.com**: solicitar review de un pull request.

Y entender cómo personalizar ambos con archivos de instrucciones.

## Conceptos clave

GitHub Copilot tiene dos surfaces de code review distintos que la documentación a veces mezcla:

| Surface | Dónde vive | Para qué sirve |
|---------|-----------|----------------|
| Review en VS Code | Editor local | Feedback inmediato sobre el código que estás escribiendo, antes de commitear |
| Review en GitHub.com | Pull request en la web | Revisión asíncrona del PR completo, con comentarios inline |

Ambos comparten la misma fuente de personalización: los archivos de instrucciones. Pero responden a triggers distintos y tienen limitaciones distintas.

## Limitación importante de code review en PRs

Code review en pull requests **solo lee los primeros 4000 caracteres** de cada archivo de instrucciones. Esto no aplica al chat ni al coding agent. Si tu `copilot-instructions.md` tiene 8000 caracteres, las reglas que estén después del corte simplemente no se aplican en PRs.

La consecuencia práctica: las reglas más críticas para code review deben ir al principio del archivo, o moverse a archivos `*.instructions.md` específicos para code review. Para esto último, el frontmatter `applyTo` con un patrón apropiado mantiene la regla disponible solo donde corresponde.

## Paso 6.1: Code review en VS Code para un archivo

1. Abre `Services/PrestamoServicio.cs`.
2. Selecciona todo el contenido (`Ctrl+A` o `Cmd+A`).
3. Clic derecho > **Copilot** > **Review and Comment**.

VS Code abre el panel de comentarios y muestra los hallazgos inline en el editor. Cada hallazgo tiene un botón para aplicar la corrección sugerida.

Como el `PrestamoServicio.cs` del starter tiene bugs deliberados, deberías ver al menos:

- Uso de `double` donde debería ser `decimal`.
- Fórmula de amortización incorrecta o incompleta.
- Falta de validaciones de entrada.

Si no ves esos hallazgos, las instrucciones que configuraste en el módulo 2 no se están aplicando al review. Ajusta `copilot-instructions.md` para que mencione explícitamente las reglas financieras y vuelve a correr.

## Paso 6.2: Configurar instrucciones específicas para code review

Esta es la parte que muchos no aprovechan. Puedes crear un archivo de instrucciones que solo se aplique al code review (y no al chat normal) usando el campo `excludeAgent` del frontmatter.

Crea `.github/instructions/code-review.instructions.md`:

```markdown
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
```

El campo `excludeAgent: ['vscode']` significa que esta regla no se aplica al chat de VS Code, solo al code review en GitHub.com y al coding agent. Esto evita que al chatear normalmente, Copilot empiece a clasificar todo por severidad.

Importante: el soporte de `excludeAgent` y `applyTo` en code review aún se está estabilizando. Si ves comportamientos inconsistentes, revisa el log de actions de Copilot en el PR para confirmar qué archivos cargó.

## Paso 6.3: Activar code review automático en el repo

En GitHub.com:

1. Ve a tu repositorio > **Settings** > **Code & automation** > **Copilot**.
2. Busca **Copilot code review** y habilita "Auto-review on pull request".

Si no ves esa opción, tu organización no tiene Copilot Business o Enterprise habilitado, o no te dieron permiso. Pídele al admin que habilite la política "Copilot code review".

Para repos donde no puedes activar auto-review (cuenta personal sin Copilot Pro+), todavía puedes pedir review manual: en el PR, en la sección de Reviewers, asigna a Copilot.

## Paso 6.4: Crear un PR de prueba

```bash
git checkout -b feature/agregar-validacion-rangos
```

Modifica `Services/PrestamoServicio.cs` agregando una validación deliberadamente incompleta:

```csharp
public decimal Calcular(decimal monto, decimal tasa, int plazo)
{
    if (monto < 0) throw new ArgumentException("Monto inválido");
    // ... resto del código existente sin cambios
}
```

Notar que la validación es incompleta: no chequea el límite superior, no chequea el plazo, no chequea la tasa. El review debería detectarlo.

```bash
git add . && git commit -m "feat: agregar validacion parcial de monto"
git push -u origin feature/agregar-validacion-rangos
```

En GitHub, abre el PR y solicita review de Copilot. En aproximadamente 30 segundos, deberías ver comentarios inline señalando las validaciones faltantes según las reglas de severidad alta de tu archivo de instrucciones.

## Paso 6.5: Iterar sobre las instrucciones

Si el review no detecta lo que esperabas, las causas comunes son:

1. **Reglas demasiado vagas**: "asegurarse de que el código es seguro" no produce hallazgos específicos. Las reglas deben ser verificables.

2. **Demasiadas reglas**: si tienes 30 reglas, el modelo prioriza algunas y deja otras. Reduce a 10-15 críticas.

3. **Reglas contradictorias**: si una regla dice "evita early returns" y otra dice "guard clauses al inicio", el modelo se confunde.

4. **Archivo demasiado largo**: recuerda el límite de 4000 caracteres para code review en PRs.

Itera el archivo de instrucciones, abre nuevos PRs (o pide nueva review en el actual), y observa qué cambia. Es un proceso de calibración que dura semanas en repos reales.

## Paso 6.6: Custom prompt para code review focalizado

Cuando quieres una revisión profunda de algo específico (ejemplo: "revisa la seguridad de los endpoints públicos"), crear un prompt file dedicado funciona mejor que pedir review general.

Crea `.github/prompts/revisar-endpoints.prompt.md`:

```markdown
---
description: Revisa los endpoints HTTP del repo enfocándose en seguridad y validación
agent: agent
model: Claude Opus 4.5 (copilot)
---

# Revisión enfocada de endpoints

Lee todos los archivos en `Endpoints/` y `Program.cs`. Para cada endpoint
encontrado, evalúa:

1. **Validación de entrada**: el endpoint valida tipos, rangos, formato antes
   de invocar al servicio?

2. **Códigos de respuesta**: el endpoint declara `Produces<T>` para todos los
   códigos posibles? Se devuelven los códigos correctos (200, 201 con
   Location, 204, 400, 404, 422)?

3. **Información en respuestas de error**: los mensajes de error son
   informativos sin filtrar detalles internos del sistema (stacktraces,
   nombres de tablas, paths de archivos)?

4. **Idempotencia**: los métodos GET son realmente idempotentes? Los DELETE
   manejan correctamente el caso de no encontrado?

5. **Autorización**: hay endpoints que deberían requerir autorización y no
   la requieren? (En este proyecto educativo no hay auth implementada, así
   que solo señala dónde habría que agregarla en producción).

## Formato

Tabla por endpoint con columnas: Endpoint, Verbo+Ruta, Severidad del problema
más alto encontrado, Resumen. Después, una sección por endpoint con detalle
de hallazgos.

No reportes endpoints que están bien. Solo los que tienen problemas.
```

Úsalo con `/revisar-endpoints`. Esto produce una revisión más profunda y enfocada que el code review general, y queda versionado en el repo para que cualquiera del equipo la corra.

## Diferencias con code review en VS Code

El code review en VS Code (clic derecho > Review and Comment) es:

- Más rápido y barato (no consume premium requests si está incluido en tu plan).
- Limitado al archivo o selección actual.
- Más inmediato: puedes iterar al instante.

El code review en GitHub.com es:

- Asíncrono y cubre el PR completo.
- Funciona para colaboradores que no tienen Copilot instalado en su IDE.
- Consume premium requests (verifica las políticas de tu organización).

En la práctica los uso en distintos momentos: VS Code mientras escribo, GitHub.com como gate antes del merge.

## Limitaciones que conviene conocer

Copilot code review en PRs es excelente para detectar problemas mecánicos (bugs comunes, patrones de seguridad, violaciones de estilo) pero **no entiende tu dominio de negocio**. Un humano sigue siendo necesario para revisar:

- Decisiones de arquitectura.
- Coherencia con reglas de negocio que no están explícitas en código.
- Trade-offs de diseño.
- Contexto histórico ("esto se hace así porque en 2023 pasó X").

Tratar el review de Copilot como reemplazo de revisión humana es un error. Tratarlo como un primer pase que filtra lo evidente para que el humano se concentre en lo importante es el uso correcto.

## Siguiente

[Módulo 7: Cuándo usar qué](07-cuando-usar-que.md)
