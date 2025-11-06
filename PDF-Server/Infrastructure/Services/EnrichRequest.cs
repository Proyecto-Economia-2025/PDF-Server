
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;

namespace PDF_Server.Infrastructure.Services
{
    public class RequestEnricher : IRequestEnricher
    {
        public BaseRequest EnrichRequest(BaseRequest request)
        {
            if (request == null) return null;

            // Sobrescribimos el Timestamp con la hora actual
            request.Timestamp = DateTime.UtcNow;

            // Sobrescribimos el ServerHost con el nombre del servidor actual
            request.ServerHost = Environment.MachineName;

            //Sobrescrimos Service con el nombre del ensamblado actual
            request.Service = "PDF Server";

            //Sobrescribimos Endpoint 
            request.Endpoint = "/api/PDF/get-top-products";

            return request;

        }
    }
}
