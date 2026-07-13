---
name: scoring-crediticio
description: Clasifica el riesgo crediticio de un solicitante de préstamo de Contoso Banco aplicando la tabla de scoring CB-RISK-2024 y las reglas duras que pueden sobrescribir el puntaje. Usa esta skill cuando se pida evaluar, clasificar, aprobar o rechazar una solicitud, o calcular el score de riesgo de un cliente.
---

# Scoring crediticio de Contoso Banco (CB-RISK-2024)

Esta skill encapsula la metodología con la que Contoso Banco decide si aprueba,
manda a revisión o rechaza una solicitud de préstamo. **No calcula la cuota**
(eso es otro proceso): decide el riesgo del solicitante.

Úsala cuando el usuario pida:

- Evaluar o clasificar una solicitud de préstamo.
- Calcular el score de riesgo de un cliente.
- Explicar por qué una solicitud fue aprobada o rechazada.
- Implementar o validar el código de scoring.

## El modelo tiene dos capas

El punto pedagógico de esta skill: un modelo de riesgo real **no es solo una
suma de puntos**. Primero se calcula un score numérico y después unas **reglas
duras** pueden sobrescribir la decisión. Un solicitante puede tener un score
excelente y aun así ser rechazado por una regla dura. Implementar solo la suma
y olvidar las reglas es el bug clásico.

## Capa 1: tabla de puntaje

Score base: **500**. Se suman los puntos de cada factor.

**Relación cuota/ingreso (DTI = cuota mensual / ingreso mensual):**

| DTI | Puntos |
|-----|--------|
| ≤ 0.20 | +150 |
| > 0.20 y ≤ 0.35 | +80 |
| > 0.35 y ≤ 0.45 | +20 |
| > 0.45 | −120 |

**Historial de pagos:**

| Historial | Puntos |
|-----------|--------|
| excelente (sin atrasos) | +120 |
| bueno (atrasos < 30 días) | +50 |
| regular (atrasos 30–90 días) | −30 |
| malo (atrasos > 90 días o default) | −60 |

**Antigüedad laboral (en meses):**

| Antigüedad | Puntos |
|------------|--------|
| ≥ 36 | +80 |
| ≥ 12 y < 36 | +40 |
| ≥ 6 y < 12 | +10 |
| < 6 | −60 |

**Relación monto/ingreso anual (monto / (ingreso mensual × 12)):**

| Relación | Puntos |
|----------|--------|
| ≤ 1.0 | +50 |
| > 1.0 y ≤ 2.0 | +10 |
| > 2.0 | −70 |

El score final se recorta (clamp) al rango **[300, 850]**.

## Capa 1 (continuación): bandas de decisión

El score se mapea a una banda:

| Score | Banda | Decisión |
|-------|-------|----------|
| ≥ 720 | A | Aprobado automático |
| 660–719 | B | Aprobado con revisión estándar |
| 600–659 | C | Revisión manual obligatoria |
| < 600 | D | Rechazado |

## Capa 2: reglas duras (sobrescriben la banda)

Se evalúan **después** de calcular el score y **pueden degradar la decisión**,
nunca mejorarla:

- **RD-1**: si el historial es `malo`, la decisión final es **Banda D
  (Rechazado)**, sin importar el score. Contoso no presta a clientes en default
  aunque el resto del perfil sea fuerte.
- **RD-2**: si el DTI > 0.45, la banda final **no puede ser mejor que C**. Si el
  score daba A o B, se degrada a C (revisión manual). Nunca aprobado automático
  con esa carga de deuda.

Reporta siempre **el score calculado, la banda por score y la decisión final**,
y si una regla dura cambió el resultado, indícalo explícitamente (por ejemplo:
"score 720 → banda A por puntaje, pero RD-1 la degrada a D por historial malo").

## Reglas operativas

- Todos los cálculos intermedios en `decimal`. El DTI y la relación de monto son
  `decimal`, no `double`.
- El score final es un entero (`int`).
- Rechaza entradas fuera de rango antes de puntuar: ingreso mensual > 0,
  cuota mensual > 0, antigüedad ≥ 0, historial en el conjunto
  {excelente, bueno, regular, malo}.

## Validación de tu propio trabajo

Después de generar código de scoring, ejecuta el script
[validar-scoring.csx](./validar-scoring.csx). Corre los tres casos estándar y
confirma que tu implementación produce el mismo score y la misma decisión final:

```
dotnet script .github/skills/scoring-crediticio/validar-scoring.csx
```

Si los resultados no coinciden con la salida esperada (ver
[ejemplos/caso-scoring.json](./ejemplos/caso-scoring.json)), tu implementación
tiene un bug. El error más común: olvidar la capa 2 y aprobar el caso 3.

## Casos de prueba estándar

| Caso | Ingreso | Cuota | DTI | Historial | Antigüedad | Monto | Score | Banda score | Decisión final |
|------|---------|-------|-----|-----------|------------|-------|-------|-------------|----------------|
| 1 | 30000 | 6000 | 0.20 | excelente | 48 | 100000 | 850 | A | Aprobado automático |
| 2 | 20000 | 8000 | 0.40 | bueno | 18 | 250000 | 620 | C | Revisión manual |
| 3 | 80000 | 12000 | 0.15 | malo | 60 | 100000 | 720 | A | **Rechazado (RD-1)** |

El caso 3 es la prueba clave: el score da 720 (banda A), pero la regla dura RD-1
lo rechaza por historial `malo`. Si tu implementación aprueba el caso 3, le falta
la capa 2. No lo aceptes como "casi correcto".
