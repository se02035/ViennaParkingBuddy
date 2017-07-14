using Geocoding;
using Geocoding.Microsoft;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace VPB.Bot.Helper
{
    [Serializable]
    public class VerifiedAddress
    {
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    internal class GeoHelper
    {
        private static string BingMapsApiKey = ConfigurationManager.AppSettings["BingMapsApiKey"];
        private static BingMapsGeocoder _geoCoder = new BingMapsGeocoder(BingMapsApiKey);

        internal static async Task<IEnumerable<Address>> LookupAsync(string address)
        {
            if (String.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Invalid parameter", address);
            }

            return await _geoCoder.GeocodeAsync(address);
        }

        internal static IEnumerable<BingAddress> Lookup(string address)
        {
            if (String.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Invalid parameter", address);
            }

            return _geoCoder.Geocode(address);
        }
 
        internal static async Task<IEnumerable<BingAddress>> GetAddressAsync(double longitude, double latitude)
        {
            return await _geoCoder.ReverseGeocodeAsync(latitude, longitude);
        }

        internal static IEnumerable<BingAddress> GetAddress(double longitude, double latitude)
        {
            return _geoCoder.ReverseGeocode(latitude, longitude);
        }

    }
}