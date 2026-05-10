using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tutorial.Entities;
using Route = Tutorial.Entities.Route;

namespace Tutorial.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core'un standart işlemlerini yapması için base metod çağrılır
            base.OnModelCreating(modelBuilder);

            // TripStatus için string dönüşümü
            modelBuilder.Entity<Trip>()
                .Property(t => t.TripStatus)
                .HasConversion<string>();

            // ReservationStatus için string dönüşümü
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ReservationStatus)
                .HasConversion<string>();

            // PassengerStatus için string dönüşümü
            modelBuilder.Entity<Reservation>()
                .Property(p => p.PassengerStatus)
                .HasConversion<string>();

            // Aktif olmayan firmaları getirmemesi için bir filtre ekliyoruz.
            modelBuilder.Entity<Company>().HasQueryFilter(c => c.IsActive);
        }
    }
}
