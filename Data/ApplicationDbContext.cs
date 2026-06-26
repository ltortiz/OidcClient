using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data
{
    // Heredamos de DbContext para heredar todo el poder de Entity Framework Core
    public class ApplicationDbContext : DbContext
    {
        // El constructor recibe la configuración (como la cadena de conexión)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Declaramos los sets de datos que representan nuestras tablas
        public DbSet<Person> Persons { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
