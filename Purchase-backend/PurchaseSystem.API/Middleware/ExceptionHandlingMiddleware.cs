namespace PurchaseSystem.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new
            {
                Success = false,
                Message = "系统内部错误，请稍后重试",
                Error = exception.Message,
                StackTrace = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                    ? exception.StackTrace
                    : null
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
