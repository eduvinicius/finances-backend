using System.Net;
using MyFinances.Api.Responses;

namespace MyFinances.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware(
           RequestDelegate next,
           ILogger<GlobalExceptionHandlerMiddleware> logger,
           IHostEnvironment env)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;
        private readonly IHostEnvironment _env = env;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case MyFinancesException customException:
                    // Exceções customizadas do domínio
                    response.StatusCode = customException.StatusCode;
                    errorResponse.StatusCode = customException.StatusCode;
                    errorResponse.Message = customException.Message;
                    _logger.LogWarning(exception, "Business exception: {Message}", customException.Message);
                    break;

                case KeyNotFoundException keyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = keyNotFoundException.Message;
                    _logger.LogWarning(exception, "Resource not found: {Message}", keyNotFoundException.Message);
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Message = "Você não tem permissão para acessar este recurso";
                    _logger.LogWarning(exception, "Unauthorized access attempt");
                    break;

                case DbUpdateException dbException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = "Um conflito no banco de dados ocorreu. O recurso pode já existir ou ter dependências.";
                    _logger.LogError(exception, "Database update exception: {Message}", dbException.Message);
                    break;

                case ArgumentException argumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = argumentException.Message;
                    _logger.LogWarning(exception, "Invalid argument: {Message}", argumentException.Message);
                    break;

                case InvalidOperationException invalidOpException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = invalidOpException.Message;
                    _logger.LogWarning(exception, "Invalid operation: {Message}", invalidOpException.Message);
                    break;

                default:
                    // Exceções não tratadas
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = _env.IsDevelopment()
                        ? exception.Message
                        : "Um erro interno do servidor ocorreu. Por favor, tente novamente mais tarde.";

                    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, options);
            await response.WriteAsync(json);
        }
    }
}
