using FluentValidation;
using PaymentRoutingEngine.Api.Contracts.Errors;
using System.Net;
using System.Text.Json;

namespace PaymentRoutingEngine.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

            var (statusCode, errors) = MapException(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ErrorResponse(traceId, errors);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static (int StatusCode, List<ApiError> Errors) MapException(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    validationException.Errors
                        .Select(e => new ApiError(
                            code: e.PropertyName,
                            message: e.ErrorMessage))
                        .ToList()
                ),

                ArgumentException argumentException => (
                    StatusCodes.Status400BadRequest,
                    new List<ApiError>
                    {
                    new ApiError("bad_request", argumentException.Message)
                    }
                ),

                KeyNotFoundException notFoundException => (
                    StatusCodes.Status404NotFound,
                    new List<ApiError>
                    {
                    new ApiError("not_found", notFoundException.Message)
                    }
                ),

                InvalidOperationException invalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    new List<ApiError>
                    {
                    new ApiError("invalid_operation", invalidOperationException.Message)
                    }
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    new List<ApiError>
                    {
                    new ApiError("internal_server_error", "An unexpected error occurred.")
                    }
                )
            };
        }
    }
}
