using GeoJSON.Net.Feature;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPB.Loader
{
    public class CosmosDbAdapter
    {
        private const string Endpoint = @"https://vpb.documents.azure.com:443/";
        private const string Authkey = @"SiW3MDRYNiffBCOTcM4lSZcHrJu6gz4xhHdSX9d0dF7tA3Dz4KFp9gsy1W3ppcf2EpRn0sKCtaT2VACA4emPCw==";

        private const string DatabaseId = "vpb";
        private const string CollectionId = "parkingzones";

        private string _geoJson;

        public CosmosDbAdapter(string geoJson)
        {
            _geoJson = geoJson;
        }

        //internal async Task InitializeDb()
        //{
        //    await _client.CreateDatabaseAsync(new Database { Id = DatabaseId });
        //    await _client.CreateDocumentCollectionAsync(
        //        UriFactory.CreateDatabaseUri(DatabaseId),
        //        new DocumentCollection { Id = CollectionId },
        //        new RequestOptions { OfferThroughput = 1000 });
        //}

        public void LoadData()
        {
            var client = new DocumentClient(new Uri(Endpoint), Authkey);
            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(_geoJson);
            foreach (var feature in featureCollection?.Features)
            {
                var document = feature.ToString();
                client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), document).GetAwaiter().GetResult();
            }
        }
    }
}
