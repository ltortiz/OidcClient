using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

/**
 * NUEVI
 */
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IAppService, AppService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(15);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");
    options.Authority = oidcConfig["Authority"];
    options.ClientId = oidcConfig["ClientId"];
    options.ClientSecret = oidcConfig["ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;

    options.SaveTokens = true;
    options.BackchannelTimeout = TimeSpan.FromSeconds(3);

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            Console.WriteLine("Estamos en OnRedirectToIdentityProvider");
            if (context.Properties.Parameters.TryGetValue("username", out var usernameObj) && usernameObj is string username)
            {
                context.ProtocolMessage.LoginHint = username;
            }
            return Task.CompletedTask;
        },
        OnTicketReceived = async context =>
        {
            Console.WriteLine("Estamos en OnTicketReceived");
            var idToken = context.Properties.GetTokenValue("id_token") ?? "";
            Console.WriteLine(idToken);
            var loginReturnUrl = context.Properties.Items["login_return"];
            Console.WriteLine($"loginReturnUrl {loginReturnUrl}");
            var clientId = context.Properties.Items["client_id"];
            Console.WriteLine($"clientId {clientId}");

            var url = $"{loginReturnUrl}?id_token={Uri.EscapeDataString(idToken)}&client_id={clientId}";

            context.Response.Redirect(url);

            context.HandleResponse();

            await Task.CompletedTask;
        },
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            Console.WriteLine("Estamos en OnRedirectToIdentityProviderForSignOut");
            var endpoint = context.ProtocolMessage.IssuerAddress;
            Console.WriteLine($"endpoint {endpoint}");

            var postLogout = context.ProtocolMessage.PostLogoutRedirectUri;
            Console.WriteLine($"postLogout {postLogout}");

            var hint = context.ProtocolMessage.IdTokenHint;
            Console.WriteLine($"hint {hint}");

            //var idToken = await context.HttpContext.GetTokenAsync("id_token");
            var idToken = context.Properties.GetTokenValue("id_token");
            Console.WriteLine(idToken);

            if (context.Properties.Parameters.TryGetValue("id_token_hint", out var value))
            {
                Console.WriteLine($"idToken {value}");
                context.ProtocolMessage.IdTokenHint = value?.ToString();
            }

            return Task.CompletedTask;
        },
        OnSignedOutCallbackRedirect = async context => 
        {
            Console.WriteLine("Estamos en OnSignedOutCallbackRedirect");
            if (context.Properties.Items.TryGetValue("logout_return", out var logoutReturnUrl))
            {
                Console.WriteLine($"logoutReturnUrl {logoutReturnUrl}");
                context.Response.Redirect(logoutReturnUrl);
                context.HandleResponse();
            }

            await Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            Console.WriteLine("Estamos en OnTokenValidated");
            var emailClaim = context.Principal?.FindFirst(ClaimTypes.Email)?.Value ?? context.Principal?.FindFirst("email")?.Value;
            Console.WriteLine(emailClaim);

            var idToken = context.SecurityToken.RawData;
            Console.WriteLine(idToken);

            await Task.CompletedTask;
        },
        OnAuthenticationFailed = async context =>
        {
            Console.WriteLine("Estamos en OnAuthenticationFailed");
            Console.WriteLine(context.Exception.Message);

            context.HandleResponse();
            context.Response.Redirect($"/auth/error?msg={Uri.EscapeDataString(context.Exception.Message)}");

            await Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine("Estamos en OnRemoteFailure");
            string message = context.Failure?.Message ?? "";
            Console.WriteLine($"[OnRemoteFailure] Message: {message}");
            string errorType = context.Failure?.Data["error"]?.ToString() ?? "Error General";
            Console.WriteLine($"[OnRemoteFailure] Error: {errorType}");
            string errorDescription = context.Failure?.Data["error_description"]?.ToString() ?? "Ocurrió un error en la autenticación externa.";
            Console.WriteLine($"[OnRemoteFailure] Error Description: {errorDescription}");

            string messageToSend = $"({errorType}) {errorDescription}";
            if (context.Failure is TaskCanceledException || context.Failure is TimeoutException ||
        context.Failure?.InnerException is TaskCanceledException || context.Failure?.InnerException is TimeoutException)
            {
                messageToSend = "(Timeout) El inicio de sesión con OpenID no responde. Por favor, usa tus credenciales tradicionales!";
            }
            // Detectamos  si el fallo es por 503
            else if (message.Contains("503") || context.Failure?.InnerException?.Message.Contains("503") == true || errorDescription.Contains("503"))
            {
                messageToSend = "(Internal Server) El servidor de OpenID no se encuentra disponible temporalmente. Por favor, usa tus credenciales tradicionales!";
            }
            messageToSend = messageToSend.Replace("_", " ");

            context.HandleResponse();
            context.Response.Redirect($"/auth/error?msg{Uri.EscapeDataString(messageToSend)}");

            return Task.CompletedTask;
        },
    };
});
/**
 * FIN
 */

var app = builder.Build();

app.UseHttpsRedirection();

/**
 * NUEVI
 */
app.UseMiddleware<OpenIdExceptionMiddleware>();
app.UseAuthentication();
/**
 * FIN
 */

app.UseAuthorization();

app.MapControllers();

app.Run();
