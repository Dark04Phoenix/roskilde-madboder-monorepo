using System;

namespace GroenKildeApi.Models
{
    public class StationWithStatus
    {
        public Guid StationId { get; set; }
        public string? Navn { get; set; }
        public string? GPSPosition { get; set; }
        public Guid? SenesteStatusId { get; set; }
        public Guid? StatusId { get; set; }
        public string? StatusType { get; set; }
        public DateTime? StatusTidspunkt { get; set; }
        public Guid? OpdateretAf { get; set; }
    }
}
