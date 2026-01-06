using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("StationStatus")]
    public class StationStatus
    {
        [Key]
        public Guid StatusId { get; set; }

        [Required]
        public Guid StationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required]
        public DateTime Tidspunkt { get; set; }

        [Required]
        public Guid OpdateretAf { get; set; }
    }
}
