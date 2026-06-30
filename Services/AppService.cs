using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AppService : IAppService
    {
        private readonly ApplicationDbContext _db;

        public AppService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Application?> ValidateAsync(string useraname, string password)
        {
            var app = await _db.Applications.FirstOrDefaultAsync(u => u.Username == useraname);
            if (app == null || !BCrypt.Net.BCrypt.Verify(password, app.Password.Trim()))
            {
                return null;
            }
            return app;
        }

        public async Task<Application?> GetById(int id)
        {
            var app = await _db.Applications.FindAsync(id);
            return app;
        }
    }
}
