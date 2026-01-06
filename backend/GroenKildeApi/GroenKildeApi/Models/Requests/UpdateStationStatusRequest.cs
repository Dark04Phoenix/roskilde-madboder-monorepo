using System;
using System.ComponentModel.DataAnnotations;

namespace GroenKildeApi.Models.Requests
{
    public class UpdateStationStatusRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StatusType { get; set; }
    }
}
