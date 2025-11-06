using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TopProductsPdfDocument = PDF_Server.Infrastructure.PDFs.TopProductsPdfDocument;

namespace PDF_Server.Infrastructure.PDFs
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IProductRepository _productRepository;

        public PdfGeneratorService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductSale>> GetTopProductsAsync(TopProductsRequest request)
        {
            return await _productRepository.GetTopProductsAsync();
        }

        public byte[] GeneratePdfTopProducts(List<ProductSale> products, TopProductsRequest request)
        {
            if (products == null || products.Count == 0)
                throw new ArgumentException("No se recibieron productos para generar el PDF.");

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var pdfDocument = new TopProductsPdfDocument(products, request);
                    pdfDocument.GeneratePdf(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error QuestPDF] Fallo al generar el PDF: {ex.Message}");
                throw;
            }
        }
    }
}