using System.Data.Entity.Spatial;

namespace VPB.Data.Models
{
    public class TicketShop
    {
        public int Id { get; set; }
        public DbGeography Location { get; set; }

        public string ShopId { get; set; }

        public string Address { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string Caption { get; set; }
        public string ShopType { get; set; }
        public string Weblink { get; set; }
    }
}
