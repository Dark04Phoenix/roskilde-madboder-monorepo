using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("CampingUdstyr")]
    public class CampingUdstyr
    {
        [Key]
        public Guid UdstyrId { get; set; }

        [Required]
        public Guid AfleveretAf { get; set; }

        [Required]
        [Column("GenbrugsunktId")]
        public Guid GenbrugsunktId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Kategori { get; set; }

        [Required]
        [MaxLength(50)]
        public string Stand { get; set; }

        [MaxLength(100)]
        public string? Behandling { get; set; }

        [Required]
        public DateTime Tidspunkt { get; set; }
    }
}
