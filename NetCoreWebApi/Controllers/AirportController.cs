using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using AirportWebApi.Helper;
using AirportWebApi.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace AirportWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirportController : ControllerBase
    {
        #region Fields
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheKeyFormula = "{0}-{1}";
        #endregion

        #region Constructor
        public AirportController(AppDbContext context, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        #endregion

        #region API
        /// <summary>
        /// API service for calculation distance between two airport
        /// </summary>
        /// <param name="airport1">First airport's iata code</param>
        /// <param name="airport2">Second airport's iata code</param>
        /// <returns>distance in miles</returns>

        // GET api/airport/airport1/airport2
        [HttpGet("{airport1}/{airport2}")]
        public async Task<IActionResult> GetDistances(string airport1, string airport2)
        {
            try
            {
                if (string.IsNullOrEmpty(airport1) || string.IsNullOrEmpty(airport2))
                {
                    return BadRequest(new { Error = "airport code can not be null or empty", Success = false });
                }

                // Firstly, check if result is cached previously, if it's exist in cache, directly return
                string key = string.Format(_cacheKeyFormula, airport1, airport2);
                if (_memoryCache.TryGetValue(key, out double distanceCache))
                {
                    return Ok(new { Distance_Between_Airports = distanceCache + " miles", Success = true });
                }

                List<string> airports = new List<string> { airport1, airport2 };
                List<Tuple<double, double>> airportCoordinations = new List<Tuple<double, double>>();

                var taskList = new List<Task<Tuple<double, double>>>();
                foreach (string airportIataCode in airports)
                {
                    taskList.Add(
                    Task.Run(async () =>
                    {
                        return await GetAirportCoordinations(airportIataCode);
                    }));
                }
                await Task.WhenAll(taskList.ToArray());

                airportCoordinations.Add(taskList[0].Result);
                airportCoordinations.Add(taskList[1].Result);
                // Calculate distance in miles between two airports
                var distance = AirportHelper.CalculateDistanceInMiles(airportCoordinations[0].Item1, airportCoordinations[0].Item2,
                    airportCoordinations[1].Item1, airportCoordinations[1].Item2);
                // Cache result of request(airport1/airport2) 
                CacheResult(key, distance);
                //Cache result of the vice versa of request (airport2 / airport1). Because both request have same result
                key = string.Format(_cacheKeyFormula, airport2, airport1);
                CacheResult(key, distance);

                return Ok(new { Distance_Between_Airports = distance + " miles", Success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message, Success = false });
            }
        }
        #endregion

        #region Private Implementations
        /// <summary>
        /// Get the coordination information of airport asynchronously
        /// Firstly, the method checks if airport informations exist in database.
        /// If exist, directly returns coordinates from database.
        /// If not exist, it calls external api and parse informations to get coordinations
        /// On the last step, it saves informations to database for get it on the next request and avoid api calls
        /// </summary>
        /// <param name="airportIataCode">Iata code of airport</param>
        /// <returns>coordination informations in Tuple: (Item1:latitude, Item2:longitude)</returns>
        private async Task<Tuple<double, double>> GetAirportCoordinations(string airportIataCode)
        {
            using (var con = new AppDbContext(new DbContextOptions<AppDbContext>()))
            {
                var repository = new AirportRepository(con);
                double latitude = 0.0;
                double longitude = 0.0;
                var savedAirport = await repository.GetAirport(airportIataCode);
                // If airport exist in db, no need to call external api
                if (savedAirport != null)
                {
                    SetCoordinates(savedAirport, out latitude, out longitude);
                }
                else
                {
                    IRestResponse response = AirportHelper.CallApi(airportIataCode);
                    if (response.IsSuccessful == false)
                    {
                        throw new Exception(AirportHelper.ParseApiError(response));
                    }
                    var airport = AirportHelper.GetAirportFromResponse(response);
                    if (airport != null)
                    {
                        SetCoordinates(airport, out latitude, out longitude);
                        // Save airport to in-memory database using by repository
                        await repository.AddAirport(airport);
                    }
                }
                return Tuple.Create(latitude, longitude);
            }
        }
        private void SetCoordinates(Airport airport, out double latitude, out double longitude)
        {
            latitude = airport.Latitude;
            longitude = airport.Longitude;
        }
        private void CacheResult(string cacheKey, object result)
        {
            _memoryCache.Set(cacheKey, result);
        }
        #endregion
    }
}

