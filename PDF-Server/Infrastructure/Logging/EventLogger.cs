using Core.Abstractions;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;

namespace PDF_Server.Infrastructure.Logging
{
    public class EventLogger : IEventLogger
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly string _eventTopic;

        public EventLogger(IKafkaProducerService kafkaProducer, IConfiguration configuration)
        {
            _kafkaProducer = kafkaProducer;
            _eventTopic = configuration["Kafka:EventTopic"] ?? "event-logs";
        }

        public async void LogEvent(string correlationId, string service, string endpoint, string eventName, object eventData)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Service = service,
                Endpoint = endpoint,
                EventName = eventName,
                EventData = eventData,
                Type = "Event"
            };

            try
            {
                var headers = new Dictionary<string, string>
                {
                    { "CorrelationId", correlationId },
                    { "LogLevel", "Information" },
                    { "Source", service }
                };

                var jsonMessage = System.Text.Json.JsonSerializer.Serialize(logEntry, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                await _kafkaProducer.ProduceEventLogAsync(jsonMessage, headers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando log de evento a Kafka: {ex.Message}");
                Console.WriteLine($"[{DateTime.UtcNow}] CorrelationId: {correlationId} | Event: {eventName}");
            }
        }

        public void LogEvent(BaseRequest request, string eventName, object eventData)
        {
            LogEvent(request.CorrelationId, request.Service, request.Endpoint, eventName, eventData);
        }
    }
}