using System;

namespace GroenKildeApi.Models
{
    public class StationWithStatus
    {
        public Guid StationId { get; set; }
        public string? Navn { get; set; }
        public string? GPSPosition { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid? SenesteStatusId { get; set; }
    }
}
