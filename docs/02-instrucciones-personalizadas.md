# Módulo 2: Instrucciones personalizadas

Tiempo estimado: 20 minutos.

## Objetivo

Configurar dos capas de instrucciones para el repositorio:

1. Un `copilot-instructions.md` repo-wide con las reglas que aplican siempre.
2. Archivos `*.instructions.md` path-specific con `applyTo` para reglas que solo deben dispararse en ciertos archivos.

Si ya hiciste el workshop básico, el primer punto ya lo conoces. El segundo es nuevo y es donde está el valor real.

## Conceptos clave

GitHub Copilot soporta dos formatos de instrucciones:

| Archivo | Alcance | Cuándo aplica |
|---------|---------|---------------|
| `.github/copilot-instructions.md` | Todo el repo | Siempre, en cada interacción de chat, code review y coding agent |
| `.github/instructions/<algo>.instructions.md` | Específico por path | Solo cuando los archivos abiertos o editados coinciden con el patrón `applyTo` |

Esta separación importa porque las instrucciones repo-wide se cargan en cada turno. Si metes ahí reglas que solo aplican a tests, estás gastando contexto. Peor: estás contaminando las decisiones del modelo en archivos donde esas reglas no aplican.

Adicional importante: el código review de Copilot solo lee los **primeros 4000 caracteres** de cada archivo de instrucciones. No es un límite del chat, es del code review específicamente. Si tu `copilot-instructions.md` se vuelve gigante, el code review va a ignorar las reglas que estén abajo. Esto es razón suficiente para mover reglas específicas a archivos `*.instructions.md`.

## Paso 2.1: Crear las instrucciones repo-wide

Crea el archivo `.github/copilot-instructions.md` en la raíz del workspace con este contenido. Cópialo tal cual o pídele a Copilot que lo genere a partir del prompt de abajo.

```markdown
# Instrucciones generales de Contoso Banco - Servicio de Préstamos

## Idioma y comunicación

- Todo el código fuente, comentarios, documentación y mensajes de error se escriben en español.
- Excepciones permitidas en inglés: palabras técnicas estándar (Get, Post, Async, Task, Dto, Id, Api).
- Los mensajes que pueda ver el usuario final en respuestas HTTP deben ser claros y sin jerga técnica.

## Stack

- .NET 8 (LTS).
- Minimal APIs, no Controllers.
- xUnit para pruebas.
- Datos en memoria (no Entity Framework, no SQL). El proyecto es educativo.

## Convenciones de código

- Nombres en PascalCase para tipos y miembros públicos en español (CalcularCuotaMensual, MontoMaximo).
- camelCase para variables locales y parámetros (montoSolicitado, tasaInteres).
- Usa `decimal` para todo monto de dinero. Nunca `double` ni `float`.
- Usa `record` para DTOs.
- Usa `IClienteServicio` para nombres de interfaces (prefijo I).

## Reglas de negocio del dominio

- Toda tasa de interés se almacena como decimal anual (0.18m representa 18%).
- Todo monto se almacena en pesos mexicanos (MXN) sin conversión.
- El plazo se mide en meses.
- Un préstamo no puede tener monto menor a 1000 ni mayor a 5000000.
- El plazo no puede ser menor a 6 meses ni mayor a 60 meses.

## Lo que NO debe hacer Copilot

- No instales paquetes nuevos sin justificación explícita.
- No introduzcas dependencias externas (Dapper, AutoMapper, MediatR, etc) sin que se te pida.
- No agregues logging ni telemetría a menos que el prompt lo pida.
```

Guárdalo y haz commit.

## Paso 2.2: Probar que las instrucciones se aplican

Abre Copilot Chat (modo Agent, modelo Claude Sonnet o Opus) y escribe:

```
Genera un endpoint POST /api/prestamos/calcular que reciba monto, tasa y plazo,
y devuelva la cuota mensual usando la fórmula de amortización francesa.
Pónlo en un archivo nuevo dentro de Endpoints/.
```

Observa el resultado. Verifica que:

- El nombre del endpoint, parámetros y variables están en español.
- Usa `decimal` y no `double`.
- No agrega paquetes que no necesita.
- Usa Minimal API y no un Controller.

Si alguna de esas cosas falla, las instrucciones no se cargaron. Verifica que el archivo está en `.github/copilot-instructions.md` exactamente y reinicia VS Code si hace falta.

Importante: aunque las instrucciones se carguen, Copilot puede no seguirlas siempre. Es no determinista. Si una regla es crítica, refuérzala en el prompt: "Recuerda usar decimal para los montos".

## Paso 2.3: Identificar reglas que no son repo-wide

Mira el archivo que acabas de generar. Hay dos tipos de reglas conviviendo en `copilot-instructions.md`:

- Generales: idioma, stack, monedas. Aplican a todo.
- Específicas por capa: convenciones de tests, formato de commits, reglas de seguridad para endpoints públicos. **No** aplican a todo.

Por ejemplo, si añades una regla como "los métodos de prueba usan el patrón `Metodo_Escenario_ResultadoEsperado`", esa regla no tiene sentido en archivos de modelo. Si la dejas en el archivo repo-wide, Copilot la aplica donde no debe (puede sugerir nombres de métodos de modelo en ese formato) y consume contexto innecesariamente.

La solución son los archivos path-specific.

## Paso 2.4: Crear instrucciones path-specific

Crea el archivo `.github/instructions/tests.instructions.md`:

```markdown
---
applyTo: "**/*Tests.cs,**/*Tests/**/*.cs"
---

# Convenciones para pruebas

- Cada test crea su propia instancia de los servicios bajo prueba. No usar fixtures compartidas a menos que se justifique.
- Nombres de métodos de prueba en patrón `Metodo_Escenario_ResultadoEsperado`. Por ejemplo: `Calcular_MontoNegativo_LanzaArgumentException`.
- Los `Arrange / Act / Assert` se separan con líneas en blanco, no con comentarios.
- Para escenarios parametrizados usa `[Theory]` con `[InlineData]`. Evita `[MemberData]` salvo que las combinaciones sean realmente complejas.
- Las aserciones de igualdad numérica con decimales usan `Assert.Equal(esperado, actual, precision: 2)`.
- No uses Moq ni NSubstitute en este proyecto: las dependencias son simples y los servicios son in-memory. Si un servicio necesita un test double, créalo manual con una clase derivada o una implementación de la interfaz.
```

Crea también `.github/instructions/endpoints.instructions.md`:

```markdown
---
applyTo: "**/Endpoints/**/*.cs,**/Program.cs"
---

# Convenciones para endpoints HTTP

- Cada grupo de endpoints va en su propio archivo dentro de `Endpoints/`, expuesto como un método de extensión sobre `IEndpointRouteBuilder`.
- El método de extensión se llama `MapPrestamosEndpoints`, `MapClientesEndpoints`, etc.
- Cada endpoint declara `.WithName()`, `.WithTags()`, `.WithOpenApi()` y `.Produces<T>(StatusCodes.Status*)` para todas las respuestas posibles.
- Los códigos de respuesta esperados son: 200 lectura, 201 creación con header Location, 204 eliminación, 400 validación, 404 no encontrado, 422 regla de negocio violada.
- La validación de entrada se hace en el endpoint, no en el servicio. El servicio asume entradas válidas.
- Para errores de regla de negocio (ejemplo: monto fuera de rango) se devuelve `Results.UnprocessableEntity` con un objeto `{ codigo, mensaje }` en español.
```

## Paso 2.5: Sintaxis de `applyTo`

El campo `applyTo` acepta patrones glob separados por coma. Algunos ejemplos útiles:

| Patrón | Aplica a |
|--------|----------|
| `**/*.cs` | Todos los archivos C# |
| `**/Models/**/*.cs` | Todos los archivos en cualquier carpeta `Models` |
| `**/*.test.ts,**/*Tests.cs` | Tests de TypeScript y C# |
| `**/Program.cs` | Solo el archivo de arranque |
| `**/*.md` | Documentación Markdown |

Una regla que se ve mucho en repos en producción y que conviene copiar: en monorepos, separar las instrucciones por proyecto usando `applyTo: "src/proyectoA/**/*.cs"` para no contaminar a otros proyectos del mismo repo.

## Paso 2.6: Verificar que las instrucciones path-specific funcionan

1. Abre un archivo dentro de `ContosoBanco.Loans.Tests/`.
2. En Copilot Chat (modo Agent), pega este prompt:

   ```text
   Genera un test para `PrestamoServicio.Calcular` que cubra el caso de monto negativo.
   ```

3. Verifica que el nombre del método sigue el patrón `Calcular_MontoNegativo_LanzaArgumentException` y que no usa Moq.

Ahora abre `Program.cs` o un archivo en `Endpoints/` y pega este prompt:

```text
Agrega un endpoint GET /api/prestamos/{id}/cronograma que devuelva el cronograma de pagos del préstamo indicado.
```

Verifica que aparece `.WithOpenApi()` y `.Produces<>()`.

Si las reglas no se aplican, revisa:

- Que el patrón de `applyTo` realmente coincide con el archivo abierto.
- Que el frontmatter YAML está bien formado (tres guiones arriba y abajo, indentación correcta).
- En el editor de Chat Customizations (`Chat: Open Chat Customizations`), pestaña **Instructions**, deberían aparecer los archivos listados con un check verde. Si están en rojo, el frontmatter está mal.

## Trade-offs honestos

Cuándo conviene path-specific y cuándo no:

**Conviene path-specific cuando:**
- La regla solo aplica a un tipo de archivo (tests, configuración, endpoints, modelos).
- El repo es grande y las reglas repo-wide ya pasan los 4000 caracteres.
- Tienes lenguajes mezclados y necesitas reglas distintas por lenguaje.

**No conviene path-specific cuando:**
- La regla aplica a todo el código (idioma, convenciones de naming generales).
- Solo tienes un puñado de reglas. Crear cinco archivos para diez reglas totales es overkill.
- El equipo es pequeño y nadie va a mantener varios archivos.

Empieza con `copilot-instructions.md` y migra a archivos path-specific solo cuando el archivo principal pase las 100 líneas o tengas reglas que aplican a archivos muy específicos.

## Siguiente

[Módulo 3: Prompt files reutilizables](03-prompt-files.md)
