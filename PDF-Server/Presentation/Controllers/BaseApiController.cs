using Microsoft.AspNetCore.Mvc;
using PDF_Server.Domain.Interfaces;
using PDF_Server.Domain.Models;
using Core.Abstractions;
using System.Diagnostics;

namespace PDF_Server.Presentation.Controllers
{
    /// <summary>
    /// Base controller providing common error handling and logging functionality
    /// </summary>
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly IErrorLogger _errorLogger;
        protected readonly IEventLogger _eventLogger;
        protected readonly IRequestLogger _requestLogger;

        protected BaseApiController(
            IErrorLogger errorLogger,
            IEventLogger eventLogger,
            IRequestLogger requestLogger)
        {
            _errorLogger = errorLogger;
            _eventLogger = eventLogger;
            _requestLogger = requestLogger;
        }

        /// <summary>
        /// Executes an async operation with comprehensive error handling and logging
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync<TRequest, TResult>(
            TRequest request,
            Func<TRequest, Task<TResult>> operation,
            string operationName)
            where TRequest : BaseRequest
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _requestLogger.LogRequest(request, true, "Request started", operationName);

                _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    $"{operationName}Started",
                    new { }
                );

                var result = await operation(request);

                stopwatch.Stop();

                _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    $"{operationName}Completed",
                    new { ExecutionTimeMs = stopwatch.ElapsedMilliseconds }
                );

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return await HandleErrorAsync(request, ex, 400, "Invalid request parameters");
            }
            catch (InvalidOperationException ex)
            {
                return await HandleErrorAsync(request, ex, 422, "Operation cannot be completed");
            }
            catch (Exception ex)
            {
                return await HandleErrorAsync(request, ex, 500, "Internal server error");
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Executes a sync operation with comprehensive error handling and logging
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync<TRequest>(
            TRequest request,
            Func<TRequest, object> operation,
            string operationName)
            where TRequest : BaseRequest
        {
            return await ExecuteAsync<TRequest, object>(
                request,
                req => Task.FromResult(operation(req)),
                operationName
            );
        }

        /// <summary>
        /// Validates request and returns BadRequest if invalid
        /// </summary>
        protected IActionResult? ValidateRequest<TRequest>(TRequest? request)
            where TRequest : BaseRequest
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Request cannot be null"
                });
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationId))
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "CorrelationId is required"
                });
            }

            return null;
        }

        private async Task<IActionResult> HandleErrorAsync(
            BaseRequest request,
            Exception ex,
            int statusCode,
            string message)
        {
            await _errorLogger.LogError(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                ex.Message,
                ex.StackTrace ?? string.Empty
            );

            _eventLogger.LogEvent(
                request.CorrelationId,
                request.Service,
                request.Endpoint,
                "ErrorOccurred",
                new
                {
                    ErrorType = ex.GetType().Name,
                    ErrorMessage = ex.Message,
                    StatusCode = statusCode
                }
            );

            return StatusCode(statusCode, new
            {
                status = "error",
                message,
                detail = ex.Message,
                correlationId = request.CorrelationId
            });
        }
    }
}
