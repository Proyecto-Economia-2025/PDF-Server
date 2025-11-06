using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IRequestValidator
    {
        (bool isValid, string reason, string flow) Validate(BaseRequest request);
    }
}