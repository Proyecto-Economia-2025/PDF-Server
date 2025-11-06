# Controllers - Control de Errores Centralizado

## BaseApiController

La clase `BaseApiController` proporciona un manejo centralizado de errores y logging para todos los controladores de la API.

### Características

1. **Manejo Automático de Errores**: Captura y registra automáticamente todas las excepciones
2. **Logging Integrado**: Registra eventos, errores y solicitudes de manera consistente
3. **Respuestas Estandarizadas**: Retorna respuestas HTTP con formato consistente
4. **Validación de Solicitudes**: Valida automáticamente los requests entrantes

### Uso Básico

Para usar esta clase base en tus controladores:

```csharp
[Route("api/[controller]")]
public class MiController : BaseApiController
{
    private readonly IMiServicio _miServicio;

    public MiController(
        IMiServicio miServicio,
        IErrorLogger errorLogger,
        IEventLogger eventLogger,
        IRequestLogger requestLogger)
        : base(errorLogger, eventLogger, requestLogger)
    {
        _miServicio = miServicio;
    }

    [HttpPost("mi-endpoint")]
    public async Task<IActionResult> MiEndpoint([FromBody] MiRequest request)
    {
        // Validar el request
        var validationResult = ValidateRequest(request);
        if (validationResult != null)
            return validationResult;

        // Ejecutar la operación con manejo automático de errores
        return await ExecuteAsync(
            request,
            async req => await _miServicio.ProcesarAsync(req),
            "MiOperacion"
        );
    }
}
```

### Métodos Disponibles

#### `ExecuteAsync<TRequest, TResult>`

Ejecuta una operación asíncrona con manejo completo de errores y logging.

**Parámetros:**
- `request`: La solicitud que implementa `BaseRequest`
- `operation`: La función a ejecutar
- `operationName`: Nombre descriptivo de la operación para logs

**Excepciones Manejadas:**
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 422 Unprocessable Entity  
- `Exception` → 500 Internal Server Error

#### `ValidateRequest<TRequest>`

Valida que el request no sea nulo y tenga un CorrelationId válido.

**Retorna:**
- `null` si la validación es exitosa
- `BadRequest` con mensaje de error si falla

### Logging Automático

El `BaseApiController` automáticamente registra:

1. **Inicio de Operación**: Cuando comienza una operación
2. **Finalización Exitosa**: Cuando una operación se completa correctamente (incluye tiempo de ejecución)
3. **Errores**: Cuando ocurre cualquier excepción (incluye stack trace)
4. **Requests**: Todas las solicitudes entrantes

### Formato de Respuesta

Todas las respuestas de error siguen este formato:

```json
{
    "status": "error",
    "message": "Mensaje descriptivo del error",
    "detail": "Detalles técnicos del error",
    "correlationId": "uuid-de-la-solicitud"
}
```

Las respuestas exitosas mantienen su formato original definido por el servicio.

### Ventajas

- ✅ **Sin Código Duplicado**: No necesitas try-catch en cada acción del controlador
- ✅ **Logging Consistente**: Todos los errores se registran de la misma manera
- ✅ **Trazabilidad**: Cada operación se puede rastrear con su CorrelationId
- ✅ **Mantenibilidad**: Un solo lugar para modificar el comportamiento de manejo de errores
- ✅ **Testing Simplificado**: Los controladores tienen menos lógica para probar

### Ejemplo Completo

Ver `PDFController.cs` para un ejemplo completo de implementación.
