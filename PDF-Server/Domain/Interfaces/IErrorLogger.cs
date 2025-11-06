
using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IErrorLogger
    {
        Task LogError(string correlationId, string service, string endpoint, string errorMessage, string? stackTrace = null);
        Task LogError(BaseRequest request, string errorMessage, string? stackTrace = null);
    }
}
