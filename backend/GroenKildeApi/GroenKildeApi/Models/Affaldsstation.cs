using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("Affaldsstation")]
    public class Affaldsstation
    {
        [Key]
        public Guid StationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Navn { get; set; }

        [MaxLength(150)]
        public string? GPSPosition { get; set; }

        public Guid? SenesteStatusId { get; set; }
    }
}
