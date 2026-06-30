using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    [Table("applications")]
    public class Application
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Column("password")]
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Column("url_login")]
        [Required]
        [StringLength(255)]
        public string UrlLogin { get; set; } = string.Empty;

        [Column("url_logout")]
        [Required]
        [StringLength(255)]
        public string UrlLogout { get; set; } = string.Empty;

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
