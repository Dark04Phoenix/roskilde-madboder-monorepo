using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("Genbrugspunkt")]
    public class Genbrugspunkt
    {
        [Key]
        public Guid GenbrugspunktId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Navn { get; set; }

        [MaxLength(150)]
        public string? GPSPosition { get; set; }
    }
}
