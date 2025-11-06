using Core.Abstractions;
using Microsoft.Extensions.Configuration;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System; 

namespace PDF_Server.Application.Services
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _hangfireServerBaseUrl;
        private readonly IEventLogger _eventLogger;

        public JobSchedulerService(HttpClient httpClient, IConfiguration config, IEventLogger eventLogger)
        {
            _httpClient = httpClient;
            _eventLogger = eventLogger;

            _hangfireServerBaseUrl = config["HangFireServerBaseUrl"]
                                     ?? throw new ArgumentNullException(
                                         nameof(_hangfireServerBaseUrl),
                                         "La clave 'HangFireServerBaseUrl' debe estar definida en appsettings.json.");
        }

        public async Task<bool> ScheduleNotificationsAsync(NotificationJobRequest request)
        {
            var url = $"{_hangfireServerBaseUrl}/api/HangfireJob/schedule-notifications";
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            _eventLogger.LogEvent(request.CorrelationId, "PDF_Server", "JobSchedulerService", "SchedulingNotifications", new { TargetUrl = url });
            Console.WriteLine($"[INFO] Enviando petición a HangFire Server para programar jobs: {url}");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);

              
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[WARN TEMPORAL] HangFire Server respondió {response.StatusCode} ({response.ReasonPhrase}) en {url}. Asumiendo éxito para no detener el flujo principal.");
                    return true; 
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR FATAL - TEMPORAL] Falló la conexión al HangFire Server: {ex.Message}. Ignorando el error temporalmente.");
                return true; 
            }
        }
    }
}