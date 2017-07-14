using System.Data.Entity.Spatial;

namespace VPB.Data.Models
{
    public class ShortTermParkingZone
    {
        public int Id { get; set; }
        public DbGeography ParkingZone { get; set; }
        public string ZoneId { get; set; }
        public string District { get; set; }
        public string Weblink { get; set; }
        public string Period { get; set; }
        public string Duration { get; set; }
        public string EffectiveFrom { get; set; }
    }
}
