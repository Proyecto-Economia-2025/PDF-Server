using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IPdfGeneratorService
    {
        Task<List<ProductSale>> GetTopProductsAsync(TopProductsRequest request);
        byte[] GeneratePdfTopProducts(List<ProductSale> products, TopProductsRequest request);
    }
}