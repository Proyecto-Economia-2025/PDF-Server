using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IRequestLogger
    {
        void LogRequest(BaseRequest request, bool isValid, string reason, string flow);
    }
}