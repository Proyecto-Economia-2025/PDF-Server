using PDF_Server.Domain.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace PDF_Server.Application.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _localStorageUrl;

        public LocalStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _localStorageUrl = "http://localhost:5000/receive-pdf";
        }

        public async Task<bool> SendPdfToLocalStorageAsync(byte[] pdfBytes, string fileName, string correlationId)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var byteContent = new ByteArrayContent(pdfBytes);

                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(byteContent, "pdf", fileName);

                // Agregar el header X-Correlation-ID para tracking
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
                _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
                _httpClient.DefaultRequestHeaders.Add("X-PDF-FileName", fileName);

                Console.WriteLine($"Enviando PDF a {_localStorageUrl} con CorrelationId: {correlationId}");

                var response = await _httpClient.PostAsync(_localStorageUrl, content);
                response.EnsureSuccessStatusCode();

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al enviar el PDF al servidor de almacenamiento: {ex.Message}");
                return false;
            }
        }
    }
}