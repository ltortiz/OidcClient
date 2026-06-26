using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    // Le indicamos a .NET el nombre exacto de la tabla en PostgreSQL
    [Table("person")]
    public class Person
    {
        [Key] // Marca la llave primaria
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }
    }
}
