using AuthService.Models;

namespace AuthService.Services
{
    public interface IAppService
    {
        Task<Application?> ValidateAsync(string username, string password);

        Task<Application?> GetById(int id);
    }
}
