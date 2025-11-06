using Core.Abstractions;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using System.IO;
using System.Threading.Tasks;
using System;

namespace PDF_Server.Application.Services
{
    public class TopProductsService : ITopProductsService
    {
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IRequestLogger _requestLogger;
        private readonly IEventLogger _eventLogger;
        private readonly IErrorLogger _errorLogger;
        private readonly IRequestValidator _requestValidator;
        private readonly ILocalStorageService _localStorageService;
        private readonly IJobSchedulerService _jobSchedulerService; 

        private readonly IRequestEnricher _requestEnricher;

        public TopProductsService(
            IPdfGeneratorService pdfGeneratorService,
            IRequestLogger requestLogger,
            IEventLogger eventLogger,
            IErrorLogger errorLogger,
            IRequestValidator requestValidator,
            ILocalStorageService localStorageService,
            IJobSchedulerService jobSchedulerService,
            IRequestEnricher requestEnricher)
        {
            _pdfGeneratorService = pdfGeneratorService;
            _requestLogger = requestLogger;
            _eventLogger = eventLogger;
            _errorLogger = errorLogger;
            _requestValidator = requestValidator;
            _localStorageService = localStorageService;
            _jobSchedulerService = jobSchedulerService; 
            _requestEnricher = requestEnricher;
        }

        public async Task<object> PDFProcessTopProducts(TopProductsRequest request)
        {
            try
            {
                _eventLogger.LogEvent(request.CorrelationId, request.Service, request.Endpoint, "GettingTopProducts", new { });
                var products = await _pdfGeneratorService.GetTopProductsAsync(request);

                if (products == null || products.Count == 0)
                {
                    await _errorLogger.LogError(request, "No products found for the given criteria.", "");
                    return new { status = "error", message = "No se encontraron productos.", correlationId = request.CorrelationId };
                }

                _eventLogger.LogEvent(request.CorrelationId, request.Service, request.Endpoint, "GeneratingPDF", new { ProductCount = products.Count });

                byte[] pdfBytes = _pdfGeneratorService.GeneratePdfTopProducts(products, request);
                
                string fileName = $"TopProducts_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                bool pdfSentSuccessfully = await _localStorageService.SendPdfToLocalStorageAsync(pdfBytes, fileName, request.CorrelationId);

                if (!pdfSentSuccessfully)
                {
                    await _errorLogger.LogError(request, "Failed to send PDF to local storage server.", "");
                    return new { status = "error", message = "Error al enviar el PDF al servidor de almacenamiento.", correlationId = request.CorrelationId };
                }

                var jobRequest = new NotificationJobRequest
                {
                    CorrelationId = request.CorrelationId,
                    PdfFileName = fileName,

                    EmailAddress = request.EmailAddress,           
                    MessageRecipient = request.MessageRecipient,   
                    Subject = request.Subject,                    
                    MessageBody = request.MessageBody,            
                    PlatformType = request.PlatformType            
                };

                bool jobsScheduled = await _jobSchedulerService.ScheduleNotificationsAsync(jobRequest);

                if (!jobsScheduled)
                {
                    Console.WriteLine($"[WARN] {request.CorrelationId}: Falló la programación de jobs de notificación en HangFire Server.");
                    
                }

                _requestEnricher.EnrichRequest(request);

                _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    "PDFGeneratedAndJobsScheduled", 
                    new { ProductCount = products.Count, PdfSize = pdfBytes.Length }
                );

                return new
                {
                    status = "success",
                    message = "PDF creado, guardado y jobs de notificación programados correctamente.", // Mensaje final actualizado
                    productsCount = products.Count,
                    pdfSize = pdfBytes.Length,
                    fileName = fileName,
                    generatedAt = DateTime.Now,
                    correlationId = request.CorrelationId
                };
            }
            catch (Exception ex)
            {
                await _errorLogger.LogError(request, $"Error generando PDF: {ex.Message}", ex.StackTrace ?? string.Empty);
                _eventLogger.LogEvent(request.CorrelationId, request.Service, request.Endpoint, "PDFProcessingError", new { ErrorMessage = ex.Message });

                return new
                {
                    status = "error",
                    message = "Error interno del servidor al generar el PDF",
                    correlationId = request.CorrelationId
                };
            }
        }
    }
}