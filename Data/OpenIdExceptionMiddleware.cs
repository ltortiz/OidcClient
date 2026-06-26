namespace AuthService.Data
{
    public class OpenIdExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public OpenIdExceptionMiddleware(RequestDelegate next)
        {
            //"siguiente paso" se recibe para saber a donde enviar la petición al terminar
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //intenta ejecutar la petición web normal
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Middleware Atrapó Falla Real]: {ex.Message}");
                // Detectamos si la pantalla azul fue provocada por el fallo de configuración de OpenID Connect
                // IDX20803 es el código de error oficial que significa "no pude obtener el documento de configuración del servidor"
                if (ex is InvalidOperationException && ex.Message.Contains("IDX20803") ||
                    ex.InnerException is TaskCanceledException ||
                    ex.InnerException is TimeoutException)
                {
                    context.Session.SetString("SessionOidcError", "El inicio de sesión con OpenID no responde. Por favor, usa tus credenciales tradicionales.");
                    // Limpiamos la respuesta para que no dibuje la pantalla azul de error
                    context.Response.Clear();

                    // Redirigimos de forma segura al login tradicional
                    //context.Response.Redirect($"/Login?errorOidc={Uri.EscapeDataString("El inicio de sesión con OpenID no responde. Por favor, usa tus credenciales tradicionales.")}");
                    context.Response.Redirect("/Login");
                    return;
                }

                // Si es cualquier otro tipo de error, permitimos que la aplicación siga su curso normal
                throw;
            }
        }
    }
}
