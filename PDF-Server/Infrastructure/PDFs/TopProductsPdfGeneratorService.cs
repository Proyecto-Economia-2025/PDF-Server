using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;

namespace PDF_Server.Infrastructure.PDFs
{
    public class TopProductsPdfGeneratorService : IPdfGeneratorService
    {
        private readonly IProductRepository _productRepository;

        public TopProductsPdfGeneratorService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductSale>> GetTopProductsAsync(TopProductsRequest request)
        {
            Console.WriteLine("🔹 Obteniendo productos del repositorio...");
            var products = await _productRepository.GetTopProductsAsync();
            Console.WriteLine($"🔹 Se obtuvieron {products?.Count ?? 0} productos del repositorio");
            return products ?? new List<ProductSale>();
        }

        public byte[] GeneratePdfTopProducts(List<ProductSale> products, TopProductsRequest request)
        {
            try
            {
                Console.WriteLine($"🔹 GeneratePdfTopProducts iniciado con {products?.Count ?? 0} productos");

                if (products == null || products.Count == 0)
                {
                    Console.WriteLine("❌ No se recibieron productos");
                    throw new ArgumentException("No se recibieron productos");
                }

                Console.WriteLine($"🔹 Creando instancia de TopProductsPdfDocument...");
                var pdfDocument = new TopProductsPdfDocument(products, request);

                Console.WriteLine($"🔹 Generando PDF en memoria...");
                var pdfBytes = pdfDocument.GeneratePdfBytes();

                Console.WriteLine($"✅ PDF generado exitosamente en memoria ({pdfBytes.Length} bytes)");
                return pdfBytes;
            }
            catch (QuestPDF.Drawing.Exceptions.DocumentLayoutException ex)
            {
                Console.WriteLine($"❌ DocumentLayoutException en GeneratePdfTopProducts");
                Console.WriteLine($"❌ Mensaje: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw new Exception($"Error de layout en PDF: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GeneratePdfTopProducts: {ex.GetType().Name}");
                Console.WriteLine($"❌ Mensaje: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}