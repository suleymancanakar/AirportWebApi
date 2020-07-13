using Microsoft.EntityFrameworkCore;

namespace AirportWebApi.Models
{
    public class AppDbContext : DbContext
    {
        #region Constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        #endregion

        #region Properties
        public DbSet<Airport> Airport { get; set; }
        #endregion

        #region Override methods
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase(databaseName: "Airports");
                base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
