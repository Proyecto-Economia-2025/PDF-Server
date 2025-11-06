using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using System.Text;

namespace PDF_Server.Infrastructure.Validators
{
    public class RequiredFieldsRule : IValidatorRule
    {
        public string ErrorMessage => "Campos obligatorios faltantes";

        public bool Validate(BaseRequest request, StringBuilder log)
        {
            if (string.IsNullOrEmpty(request.Service) ||
                string.IsNullOrEmpty(request.Endpoint) ||
                request.Timestamp == default ||
                request.Success == null ||
                request.ExecutionTimeMs < 0 ||
                string.IsNullOrEmpty(request.ServerHost))
            {
                log.AppendLine("Faltan campos obligatorios o valores inválidos");
                return false;
            }

            log.AppendLine("Campos obligatorios validados correctamente");
            return true;
        }
    }
}