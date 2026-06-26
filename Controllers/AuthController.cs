using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(ApplicationDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("login/local")]
        public async Task<IActionResult> LoginLocal(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username ||
                    u.UsernameOidc == request.Username);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized();

            var token = _jwt.Generate(user.Username, user.PersonId);

            return Ok(new
            {
                access_token = token
            });
        }

        [HttpGet("login/oidc")]
        public IActionResult LoginOidc([FromQuery] string returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/oidc/callback"
            };

            props.Items["returnUrl"] = returnUrl ?? "";

            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("oidc/callback")]
        public async Task<IActionResult> Callback()
        {
            Console.WriteLine("Callback!!!");
            var idToken = await HttpContext.GetTokenAsync("id_token");
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var username = User.Identity?.Name;

            if (username == null)
                return Unauthorized();

            var jwt = _jwt.Generate(username, 1);

            return Ok(new
            {
                access_token = jwt
            });
        }
    }
}
