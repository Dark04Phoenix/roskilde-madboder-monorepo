using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroenKildeApi.Models
{
    [Table("GrønScore")]
    public class GronScore
    {
        [Key]
        [Column("GrønScoreId")]
        public Guid GronScoreId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int Points { get; set; }
    }
}
