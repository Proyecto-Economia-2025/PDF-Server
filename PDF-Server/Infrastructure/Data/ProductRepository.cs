using Dapper;
using Microsoft.Data.SqlClient;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;

namespace PDF_Server.Infrastructure.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PDFConnection")
                ?? throw new ArgumentNullException("Falta la cadena de conexión en appsettings.json");
        }

        public async Task<List<ProductSale>> GetTopProductsAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                SELECT TOP 10 
                    p.ProductID AS ProductId,
                    p.Name,
                    SUM(sod.OrderQty) AS TotalSold
                FROM Sales.SalesOrderDetail sod
                INNER JOIN Production.Product p ON sod.ProductID = p.ProductID
                GROUP BY p.ProductID, p.Name
                ORDER BY TotalSold DESC;";

            var products = await connection.QueryAsync<ProductSale>(query);
            return products.ToList();
        }
    }
}