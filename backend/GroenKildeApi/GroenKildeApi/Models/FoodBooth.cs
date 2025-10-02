namespace GroenKildeApi.Models
{
    public class FoodBooth
    {
        public int Booth_ID { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CuisineType { get; set; }
        public string Description { get; set; }
    }
}
