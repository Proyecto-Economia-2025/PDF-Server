using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IRequestEnricher
    {
        /// <summary>
        /// Llena o actualiza la información de BaseRequest antes de encolar el job
        /// </summary>
        /// <param name="request">Request original</param>
        BaseRequest EnrichRequest(BaseRequest request);
    }
}
