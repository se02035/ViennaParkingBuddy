using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Diagnostics;
using Newtonsoft.Json;
using System;
using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Feature;
using System.Data.Entity.Spatial;
using VPB.Data;
using VPB.Data.Models;

namespace VPB.Loader
{
    public static class LoadParkingInformation
    {
        private const string UrlParkingZones = @"http://data.wien.gv.at/daten/geo?service=WFS&request=GetFeature&version=1.1.0&typeName=ogdwien:KURZPARKZONEOGD&srsName=EPSG:4326&outputFormat=json";
        private const string UrlTicketShops = @"http://data.wien.gv.at/daten/geo?service=WFS&request=GetFeature&version=1.1.0&typeName=ogdwien:PARKENVERKAUFOGD,ogdwien:PARKENAUTOMATOGD&srsName=EPSG:4326&outputFormat=json";


        [FunctionName("HttpTriggerCSharp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //// parse query parameter
            //string name = req.GetQueryNameValuePairs()
            //    .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //    .Value;

            //// Get request body
            //dynamic data = await req.Content.ReadAsAsync<object>();

            //// Set name to query string or body data
            //name = name ?? data?.name;

            //var result = name == null
            //    ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
            //    : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);

            // parking buddy
            //await ImportDataSql();
            await ImportDataCosmos();

            return req.CreateResponse(HttpStatusCode.OK, "Successfully finished data import"); ;
        }

        private static async Task ImportDataCosmos()
        {
            var jsonParkingZones = await GetJson(UrlParkingZones);
            var cosmosdb = new CosmosDbAdapter(jsonParkingZones);
            cosmosdb.LoadData();
        }

        private static async Task ImportDataSql()
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
                            ctx.TicketShop.Add(new TicketShop()
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