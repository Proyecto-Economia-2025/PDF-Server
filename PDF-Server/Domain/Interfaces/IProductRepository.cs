using PDF_Server.Domain.Models;

namespace PDF_Server.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<List<ProductSale>> GetTopProductsAsync();
    }
}