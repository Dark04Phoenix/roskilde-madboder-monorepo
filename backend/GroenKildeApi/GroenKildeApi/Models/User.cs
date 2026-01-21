using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Navn { get; set; }

        [Required]
        [MaxLength(20)]
        public string Rolle { get; set; }

        [Required]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
