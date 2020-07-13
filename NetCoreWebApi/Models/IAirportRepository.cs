using System.Threading.Tasks;

namespace AirportWebApi.Models
{
    public interface IAirportRepository
    {
        Task<Airport> GetAirport(string airportCode);
        Task<Airport> AddAirport(Airport airport);
    }
}
