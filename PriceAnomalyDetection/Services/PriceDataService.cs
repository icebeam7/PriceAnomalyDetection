using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;

using PriceAnomalyDetection.Models;
using PriceAnomalyDetection.Helpers;

namespace PriceAnomalyDetection.Services
{
    public static class PriceDataService
    {
        public static List<Series> GetPriceDataSeries()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(PriceDataService)).Assembly;
            var stream = assembly.GetManifestResourceStream($"{Constants.ProjectName}.Data.priceData.json");

            var text = string.Empty;

            using (var reader = new System.IO.StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<List<Series>>(text);
        }
    }
}
