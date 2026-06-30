namespace AuthService.Data
{
    public class OpenIdExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public OpenIdExceptionMiddleware(RequestDelegate next)
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
                Console.WriteLine($"[Middleware Atrapó Falla Real]: {ex.Message}");
                context.Response.Clear();
                string message = ex.Message;
                if (ex is InvalidOperationException && ex.Message.Contains("IDX20803") ||
                    ex.InnerException is TaskCanceledException ||
                    ex.InnerException is TimeoutException)
                {
                    message = "El inicio de sesión con OpenID no responde. Por favor, usa tus credenciales tradicionales.";
                }
                context.Response.Redirect($"/auth/error?msg={Uri.EscapeDataString(message)}");

            }
        }
    }
}
