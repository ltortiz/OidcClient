namespace AuthService.Services
{
    public class JwtService
    {
        public string Generate(string username, int personId)
        {
            return Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"token-{username}-{personId}-{Guid.NewGuid()}"));
        }
    }
}
