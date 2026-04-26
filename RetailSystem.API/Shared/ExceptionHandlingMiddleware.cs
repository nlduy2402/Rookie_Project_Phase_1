namespace RetailSystem.API.Shared
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "SYSTEM ERRORS");

            context.Response.ContentType = "application/json";

            var response = new
            {
                Success = false,
                Title = "Error Info",
                Status = 500,
                Message = exception switch
                {
                    Microsoft.Data.SqlClient.SqlException => "Cannot Connect With Database.Check The Connection",
                    TimeoutException => "Time Out Process.",
                    _ => "Unknown Error."
                }
            };

            context.Response.StatusCode = 500;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
