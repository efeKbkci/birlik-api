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
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // EF Core'un standart işlemlerini yapması için base metod çağrılır
            base.OnModelCreating(modelBuilder);

            /* Bir enum class veritabanına yazılırken int olarak yazılır. Örneğin TripStatus.OnSale, 2 olarak veri tabanına yazılır. 
               Bu işlem okunabilirliği azaltır. Bu nedenle biz int olarak değil, string olarak yazmak istiyoruz. 
               Artık TripStatus.OnSale veri tabanına "OnSale" olarak yazılıyor. 
             */

            // TripStatus için string dönüşümü
            modelBuilder.Entity<Trip>()
                .Property(t => t.TripStatus)
                .HasConversion<string>();

            // ReservationStatus için string dönüşümü
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ReservationStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Driver>()
                .Property(d => d.Status)
                .HasConversion<string>();

            /*
            // PassengerStatus için string dönüşümü
            modelBuilder.Entity<Reservation>()
                .Property(p => p.PassengerStatus)
                .HasConversion<string>();

            */

            // Aktif olmayan firmaları getirmemesi için bir filtre ekliyoruz.
            modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Driver>().HasQueryFilter(d => !d.IsDeleted); // !d.Company.IsDeleted koşulunu eklemedim.

            modelBuilder.Entity<Route>().HasQueryFilter(c => !c.IsDeleted);
            
            modelBuilder.Entity<City>().HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Vehicle>().HasQueryFilter(v => !v.IsDeleted);
            
            modelBuilder.Entity<Stop>().HasQueryFilter(s => !s.IsDeleted);

            modelBuilder.Entity<Passenger>().HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Admin>().HasQueryFilter(a => !a.IsDeleted);
        }
        // CreatedAt değeri satırlara otomatik olarak eklenir. 
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Veritabanına eklenmek üzere olan (Added) tüm nesneleri bul
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in entries)
            {
                // Eğer bu nesnenin "CreatedAt" diye bir kolonu varsa, saatini şu anki UTC yap!
                if (entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
