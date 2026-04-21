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
