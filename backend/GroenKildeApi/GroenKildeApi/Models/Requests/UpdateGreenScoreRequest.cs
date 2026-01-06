using System.ComponentModel.DataAnnotations;

namespace GroenKildeApi.Models.Requests
{
    public class UpdateGreenScoreRequest
    {
        [Required]
        public int Points { get; set; }
    }
}
