using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;

namespace WeatherApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<WeatherData> WeatherDataSet { get; set; }
        public DbSet<UserWeatherFavorite> UserWeatherFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>()
                .Property(c => c.Latitude)
                .HasPrecision(10, 6);

            modelBuilder.Entity<City>()
                .Property(c => c.Longitude)
                .HasPrecision(10, 6);

            modelBuilder.Entity<WeatherData>()
                .Property(w => w.Temperature)
                .HasPrecision(5, 2);

            modelBuilder.Entity<WeatherData>()
                .Property(w => w.WindSpeed)
                .HasPrecision(5, 2);

            // Seed Cities
            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "Moscow", Country = "Russia", Latitude = 55.7558m, Longitude = 37.6173m, CreatedDate = new DateTime(2024, 1, 1) },
                new City { Id = 2, Name = "London", Country = "UK", Latitude = 51.5074m, Longitude = -0.1278m, CreatedDate = new DateTime(2024, 1, 1) },
                new City { Id = 3, Name = "New York", Country = "USA", Latitude = 40.7128m, Longitude = -74.0060m, CreatedDate = new DateTime(2024, 1, 1) }
            );

            // Relationships
            modelBuilder.Entity<WeatherData>()
                .HasOne(w => w.City)
                .WithMany(c => c.WeatherHistory)
                .HasForeignKey(w => w.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWeatherFavorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoriteCities)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserWeatherFavorite>()
                .HasOne(f => f.City)
                .WithMany()
                .HasForeignKey(f => f.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}