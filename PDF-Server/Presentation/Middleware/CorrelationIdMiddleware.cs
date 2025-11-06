using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Infrastructure.Messaging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PDF_Server.Presentation.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly string _logTopic;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger,
            IKafkaProducerService kafkaProducer,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _logTopic = configuration["Kafka:LogTopic"] ?? KafkaTopics.RequestLogs;
        }

        public async Task Invoke(HttpContext context)
        {
            // Obtener el correlation ID del header o generar uno nuevo
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            var isNewCorrelationId = string.IsNullOrEmpty(correlationId);

            if (isNewCorrelationId)
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Append("X-Correlation-ID", correlationId);
            }

            // Agregar información del servicio PDF a los headers
            context.Request.Headers["X-Service"] = "PDF Server";
            context.Request.Headers["X-Endpoint"] = context.Request.Path.Value ?? string.Empty;
            context.Request.Headers["X-Timestamp"] = DateTime.UtcNow.ToString("o");
            context.Request.Headers["X-Server-Host"] = Environment.MachineName;

            // Agregar el correlation ID y metadata a los headers de respuesta
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Correlation-ID", correlationId);
                context.Response.Headers.Append("X-Service", "PDF Server");
                context.Response.Headers.Append("X-Server-Host", Environment.MachineName);
                return Task.CompletedTask;
            });

            // Loggear el inicio del request
            var timestamp = DateTime.UtcNow;
            _logger.LogInformation(
                "Iniciando request {CorrelationId} - Servicio: PDF Server, Método: {Method}, Ruta: {Path}, Origen: {Origin}, Timestamp: {Timestamp}",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                isNewCorrelationId ? "Generado" : "Recibido",
                timestamp.ToString("o"));

            // Medir el tiempo de procesamiento
            var stopwatch = Stopwatch.StartNew(); try
            {
                // Continuar con el pipeline
                await _next(context);

                stopwatch.Stop();

                // Loggear la finalización exitosa
                var isSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300;
                _logger.LogInformation(
                    "Request completado {CorrelationId} - Servicio: PDF Server, Status: {StatusCode}, Duración: {ElapsedMs}ms, Servidor: {ServerHost}, Success: {Success}",
                    correlationId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    Environment.MachineName,
                    isSuccess);

                // Enviar log de finalización a Kafka
                await SendRequestLogToKafka(
                    correlationId ?? string.Empty,
                    context.Request.Path.Value ?? string.Empty,
                    isSuccess ? "VÁLIDA" : "ERROR",
                    $"Request completado con status {context.Response.StatusCode}",
                    DateTime.UtcNow,  // Timestamp al momento de completar
                    stopwatch.ElapsedMilliseconds,
                    isSuccess);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Loggear el error
                _logger.LogError(
                    ex,
                    "Error procesando request {CorrelationId} - Servicio: PDF Server, Status: {StatusCode}, Duración: {ElapsedMs}ms, Servidor: {ServerHost}, Success: false",
                    correlationId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    Environment.MachineName);

                // Enviar log de error a Kafka
                await SendRequestLogToKafka(
                    correlationId ?? string.Empty,
                    context.Request.Path.Value ?? string.Empty,
                    "ERROR",
                    $"Error: {ex.Message}",
                    DateTime.UtcNow,  // Timestamp al momento del error
                    stopwatch.ElapsedMilliseconds,
                    false);

                // Relanzar la excepción para que otros middlewares la manejen
                throw;
            }
        }

        private async Task SendRequestLogToKafka(string correlationId, string endpoint,
            string status, string reason, DateTime timestamp, long executionTimeMs, bool isSuccess)
        {
            try
            {
                var logEntry = new
                {
                    timestamp = timestamp,
                    correlationId = correlationId,
                    service = "PDF Server",
                    endpoint = endpoint,
                    status = status,
                    reason = reason,
                    validationFlow = reason,
                    serverHost = Environment.MachineName,
                    executionTimeMs = executionTimeMs,
                    isSuccess = isSuccess
                };

                var headers = new Dictionary<string, string>
                {
                    { "CorrelationId", correlationId },
                    { "LogLevel", isSuccess ? "Information" : "Error" },
                    { "Source", "PDF Server" }
                };

                await _kafkaProducer.ProduceAsync(_logTopic, logEntry, headers);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando log a Kafka para CorrelationId: {CorrelationId}", correlationId);
            }
        }
    }
}
