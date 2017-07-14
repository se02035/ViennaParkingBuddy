using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;

namespace VPB.Data
{
    [Serializable]
    public class ShortParkingZone
    {
        public string District { get; set; }
        public string Period { get; set; }
        public string Duration { get; set; }
    }

    [Serializable]
    public class TicketShop
    {
        public string Address { get; set; }
        public double Distance { get; set; }
    }

    public class DataManager
    {
        public static IEnumerable<TicketShop> GetTicketShops(double longitude, double latitude)
        {
            IEnumerable<TicketShop> result = null;

            var from = DbGeography.PointFromText($"POINT({longitude} {latitude})", 4326);
            using (var ctx = new ParkingDbContext())
            {
                result = ctx.TicketShop.
                            OrderBy(shop => shop.Location.Distance(from)).
                            Take(5).
                            Select(shop => new TicketShop()
                            {
                                Address = shop.Address,
                                Distance = (int)(shop.Location.Distance(from) ?? int.MaxValue)
                                //Id = shop.ShopId,
                                //Latitude = shop.Location.Latitude ?? int.MaxValue,
                                //Longitude = shop.Location.Longitude ?? int.MaxValue
                            }).
                            ToArray();
            }

            return result;
        }

        public static IEnumerable<ShortParkingZone> GetParkingZone(double longitude, double latitude)
        {
            IEnumerable<ShortParkingZone> result = null;

            var from = DbGeography.PointFromText($"POINT({longitude} {latitude})", 4326);
            using (var ctx = new ParkingDbContext())
            {
                result = ctx.ShortTermParkingZones
                            .Where(zone => zone.ParkingZone.Intersects(from))
                            .Select(zone =>
                                new ShortParkingZone()
                                {
                                    District = zone.District,
                                    Duration = zone.Duration,
                                    Period = zone.Period
                                })
                            .ToArray();
            }

            return result;
        }
    }
}
