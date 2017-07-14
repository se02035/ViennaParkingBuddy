using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using VPB.Data;
using VPB.Data.Models;

namespace VPB.Setup
{
    public class DbHelper
    {
        private const string UrlParkingZones = @"http://data.wien.gv.at/daten/geo?service=WFS&request=GetFeature&version=1.1.0&typeName=ogdwien:KURZPARKZONEOGD&srsName=EPSG:4326&outputFormat=json";
        private const string UrlTicketShops = @"http://data.wien.gv.at/daten/geo?service=WFS&request=GetFeature&version=1.1.0&typeName=ogdwien:PARKENVERKAUFOGD,ogdwien:PARKENAUTOMATOGD&srsName=EPSG:4326&outputFormat=json";

        static public async Task ImportData()
        {
            var jsonParkingZones = await GetJson(UrlParkingZones);
            var jsonTicketShops = await GetJson(UrlTicketShops);

            using (var ctx = new ParkingDbContext())
            {
                try
                {
                    // ============================ 
                    // Create and fill ParkingZone DB table
                    // ============================ 
                    Trace.TraceInformation($"Importing parking zones");

                    var featureCollectionZones = JsonConvert.DeserializeObject<FeatureCollection>(jsonParkingZones);
                    foreach (var item in featureCollectionZones.Features)
                    {
                        var validGeo = item.ToSqlGeometry().MakeValidIfInvalid();
                        if (validGeo.STIsValid().IsTrue)
                        {
                            ctx.ShortTermParkingZones.Add(new ShortTermParkingZone()
                            {
                                ZoneId = item.Id,
                                District = item.Properties["BEZIRK"]?.ToString(),
                                Duration = item.Properties["DAUER"]?.ToString(),
                                EffectiveFrom = item.Properties["GUELTIG_VON"]?.ToString(),
                                Period = item.Properties["ZEITRAUM"]?.ToString(),
                                Weblink = item.Properties["WEBLINK1"]?.ToString(),
                                ParkingZone = DbGeography.FromText(validGeo.ToString())
                            });
                        }
                        else
                        {
                            Trace.TraceWarning($"ParkingZone Geography '{item.Id}' is invalid");
                        }
                    }
                    Trace.TraceInformation($"Finished importing parking zones");

                    // ============================ 
                    // Create and fill TicketShop DB table
                    // ============================ 
                    Trace.TraceInformation($"Importing ticket shops");

                    var featureCollectionShops = JsonConvert.DeserializeObject<FeatureCollection>(jsonTicketShops);
                    foreach (var item in featureCollectionShops.Features)
                    {
                        var validGeo = item.ToSqlGeometry().MakeValidIfInvalid();
                        if (validGeo.STIsValid().IsTrue)
                        {
                            ctx.TicketShop.Add(new VPB.Data.Models.TicketShop()
                            {
                                ShopId = item.Id,
                                Address = item.Properties["ADRESSE"]?.ToString(),
                                Caption = item.Properties["BEZEICHNUNG"]?.ToString(),
                                District = item.Properties["BEZIRK"]?.ToString(),
                                ShopType = item.Properties["TYP"]?.ToString(),
                                Street = item.Properties["STRASSE"]?.ToString(),
                                Weblink = item.Properties["WEBLINK1"]?.ToString(),
                                Location = DbGeography.FromText(validGeo.ToString())
                            });
                        }
                        else
                        {
                            Trace.TraceWarning($"TicketShop Geography '{item.Id}' is invalid");
                        }
                    }
                    Trace.TraceInformation($"Finished importing ticket shops");

                    ctx.SaveChanges();
                    Trace.TraceInformation($"Saved to database");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error occured during import '{ex.Message}'");
                }
            }
        }

        static private async Task<string> GetJson(string url)
        {
            Trace.TraceInformation($"Requesting json data from {url}");

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
