using AirportWebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace AirportWebApi.Helper
{
    public static class AirportHelper
    {
        private static readonly string Uri = "https://places-dev.cteleport.com/airports/{0}";
        /// <summary>
        /// Calls external api of C-Teleport and gets details of corresponding airport.
        /// </summary>
        /// <param name="airportCode">IataCode of airport</param>
        /// <returns>Response that returned by C-Teleport service</returns>
        public static IRestResponse CallApi(string airportCode)
        {
            string url = string.Format(Uri, airportCode);
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return response;
        }
        /// <summary>
        /// Calculates distance between two airport's coordination
        /// </summary>
        /// <param name="lat1">latitude of first airport</param>
        /// <param name="lon1">longitute of first airport</param>
        /// <param name="lat2">latitude of second airport</param>
        /// <param name="lon2">longitute of second airport</param>
        /// <returns>distance</returns>
        public static double CalculateDistanceInMiles(double lat1, double lon1, double lat2, double lon2)
        {
            var p = Math.PI / 180;
            var a = 0.5 - Math.Cos((lat2 - lat1) * p) / 2 + Math.Cos(lat1 * p) 
                * Math.Cos(lat2 * p) * (1 - Math.Cos((lon2 - lon1) * p)) / 2;
            var kilometers = 12742 * Math.Asin(Math.Sqrt(a));
            //(1 km = 0.621371192 miles)
            //Convert to two digit
            return Math.Round(0.621371192 * kilometers, 2);
        }
        /// <summary>
        /// Creates Airport object from api response
        /// </summary>
        /// <param name="response">Response of api</param>
        /// <returns>Instance of airport</returns>
        public static Airport GetAirportFromResponse(IRestResponse response)
        {
            try
            {
                var content = DeserializeContent<JToken>(response.Content);
                if (content != null)
                {
                    Airport airport = new Airport
                    {
                        Id = Guid.NewGuid(),
                        IataCode = content["iata"].Value<string>(),
                        Latitude = content["location"]["lat"].Value<double>(),
                        Longitude = content["location"]["lon"].Value<double>()
                    };
                    return airport;
                };
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Fetch error text from api response
        /// </summary>
        /// <param name="response">Response of api</param>
        /// <returns>Error message</returns>
        public static string ParseApiError(IRestResponse response)
        {
            if (response != null)
            {
                if (response.Content.Contains("Not Found"))
                {
                    return "Not Found";
                }
                var content = DeserializeContent<JToken>(response.Content);
                if (content != null)
                {
                    return content["errors"]["msg"].Value<string>();
                }
            }
            return "Api response is returned null";
        }
        /// <summary>
        /// Deserialization of string content of api response
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="content">string content of api</param>
        /// <returns></returns>
        private static T DeserializeContent<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
