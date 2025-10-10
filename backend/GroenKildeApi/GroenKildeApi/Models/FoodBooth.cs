using System;
using System.ComponentModel.DataAnnotations;

namespace GroenKildeApi.Models
{
    public class FoodBooth
    {
        [Key]  
        public Guid Booth_ID { get; set; }

        public string Name { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string CuisineType { get; set; }
        public string Description { get; set; }
    }
}
