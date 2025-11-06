using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using PDF_Server.Infrastructure.Messaging;

namespace PDF_Server.Infrastructure.Logging
{
    public class RequestLogger : IRequestLogger
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly string _logTopic;

        public RequestLogger(IKafkaProducerService kafkaProducer, IConfiguration configuration)
        {
            _kafkaProducer = kafkaProducer;
            _logTopic = configuration["Kafka:LogTopic"] ?? KafkaTopics.RequestLogs;
        }

        public async void LogRequest(BaseRequest request, bool isValid, string reason, string flow)
        {
            string status = isValid ? "VÁLIDA" : "BLOQUEADA";

            // Crear objeto estructurado para el log
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                Status = status,
                Reason = reason,
                ValidationFlow = flow,
                request.ServerHost,
                request.ExecutionTimeMs,
                IsSuccess = request.Success
            };

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "CorrelationId", request.CorrelationId },
                    { "LogLevel", isValid ? "Information" : "Warning" },
                    { "Source", request.Service }
                };

                // Usar el método genérico para enviar el objeto como JSON
                await _kafkaProducer.ProduceAsync(_logTopic, logEntry, headers);
            }
            catch (Exception ex)
            {
                // Fallback a consola si Kafka no está disponible
                Console.WriteLine($"Error enviando a Kafka: {ex.Message}");
                Console.WriteLine($"[{DateTime.UtcNow}] CorrelationId: {request.CorrelationId} | Status: {status} | Motivo: {reason}");
                Console.WriteLine("===== Flujo de validación =====");
                Console.WriteLine(flow);
                Console.WriteLine("================================");
            }
        }
    }
}