using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAppService _appService;

        public AuthController(IAppService appService)
        {
            _appService = appService;
        }

        [HttpGet("error")]
        public IActionResult Error(string msg)
        {
            return Problem(
                title: "Error en autenticación",
                detail: $"No fue posible completar el proceso de login. Error ({Uri.UnescapeDataString(msg)})",
                statusCode: 500
            );
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] LoginRequest request)
        {
            var app = await _appService.ValidateAsync(
                request.Username,
                request.Password);

            if (app is null)
            {
                return Redirect($"/auth/error?msg={Uri.EscapeDataString("App no válida")}");
            }

            var properties = new AuthenticationProperties();
            properties.Items["login_return"] = app.UrlLogin;
            properties.Items["client_id"] = app.Id.ToString();

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string tokenId, string clientId)
        {
            //var result = await HttpContext.AuthenticateAsync();

            //var tokens = result.Properties?.GetTokens();

            //foreach (var token in tokens ?? Enumerable.Empty<AuthenticationToken>())
            //{
            //    Console.WriteLine($"{token.Name}: {token.Value}");
            //}

            //var idToken = await HttpContext.GetTokenAsync("id_token");
            //Console.WriteLine($"token {idToken}");

            //var result1 = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Console.WriteLine(result1.Succeeded);

            var app = await _appService.GetById(int.Parse(clientId));
            if (app is null)
            {
                return Redirect($"/auth/error?msg={Uri.EscapeDataString("App no válida")}");
            }

            var properties = new AuthenticationProperties();
            properties.Parameters["id_token_hint"] = tokenId;
            properties.Items["logout_return"] = app.UrlLogout;
            return SignOut(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
