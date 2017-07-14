using System.Data.Entity;

namespace VPB.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext() : base("ParkingDB") { }
        public DbSet<Models.ShortTermParkingZone> ShortTermParkingZones { get; set; }
        public DbSet<Models.TicketShop> TicketShop { get; set; }
    }
}
