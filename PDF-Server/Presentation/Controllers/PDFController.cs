using Microsoft.AspNetCore.Mvc;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using Core.Abstractions;

namespace PDF_Server.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PDFController : BaseApiController
    {
        private readonly ITopProductsService _topProductsService;

        public PDFController(
            ITopProductsService topProductsService,
            IErrorLogger errorLogger,
            IEventLogger eventLogger,
            IRequestLogger requestLogger)
            : base(errorLogger, eventLogger, requestLogger)
        {
            _topProductsService = topProductsService;
        }

        [HttpPost("get-top-products")]
        public async Task<IActionResult> GetTopProducts([FromBody] TopProductsRequest request)
        {
            var validationResult = ValidateRequest(request);
            if (validationResult != null)
                return validationResult;

            return await ExecuteAsync(
                request,
                async req => await _topProductsService.PDFProcessTopProducts(req),
                "GetTopProducts"
            );
        }
    }
}