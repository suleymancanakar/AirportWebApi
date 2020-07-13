using System.Threading.Tasks;

namespace AirportWebApi.Models
{
    public class AirportRepository : IAirportRepository
    {
        #region Fields
        private readonly AppDbContext dbContext;
        #endregion
        #region Constructor
        public AirportRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        #endregion

        #region Async db operations
        public async Task<Airport> AddAirport(Airport airport)
        {
            var result = dbContext.Airport.AddAsync(airport);
            await dbContext.SaveChangesAsync();
            return airport;
        }
        public async Task<Airport> GetAirport(string airportCode)
        {
            return await dbContext.Airport.FindAsync(airportCode);
        }
        #endregion
    }
}
