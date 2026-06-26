using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    [Table("users")]
    public class User
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

        [Column("person_id")]
        [Required]
        public int PersonId { get; set; }

        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }

        [Column("username_oidc")]
        [Required]
        [StringLength(255)]
        public string UsernameOidc { get; set; } = string.Empty;

    }
}
