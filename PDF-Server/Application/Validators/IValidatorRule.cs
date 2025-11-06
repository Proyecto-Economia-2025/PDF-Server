using PDF_Server.Domain.Models;
using System.Text;

namespace PDF_Server.Application.Validators
{
    public interface IValidatorRule
    {
        string ErrorMessage { get; }
        bool Validate(BaseRequest request, StringBuilder log);

    }
}
