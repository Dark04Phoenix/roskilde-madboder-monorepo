using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("Rapport")]
    public class Rapport
    {
        [Key]
        public Guid RapportId { get; set; }

        public Guid? StationId { get; set; }

        [Required]
        public Guid RapporteretAf { get; set; }

        [MaxLength(500)]
        public string? Beskrivelse { get; set; }

        [Required]
        [MaxLength(50)]
        public string Kategori { get; set; }

        [Required]
        public DateTime Tidspunkt { get; set; }

        [Required]
        public bool Haandteret { get; set; }
    }
}
