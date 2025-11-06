using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface ITopProductsService
    {
        Task<object> PDFProcessTopProducts(TopProductsRequest request);
    }
}