using System;
using System.ComponentModel.DataAnnotations;

namespace GroenKildeApi.Models.Requests
{
    public class CreateReportRequest
    {
        [Required]
        public Guid RapporteretAf { get; set; }

        [MaxLength(500)]
        public string? Beskrivelse { get; set; }

        [Required]
        [MaxLength(50)]
        public string Kategori { get; set; }

        public Guid? StationId { get; set; }
    }
}
