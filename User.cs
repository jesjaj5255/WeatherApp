using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Models
{
    public class User : IdentityUser
    {
        [Display(Name = "Полное имя")]
        [StringLength(100, ErrorMessage = "Полное имя не может быть больше 100 символов")]
        public string? FullName { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Избранные города")]
        public ICollection<UserWeatherFavorite> FavoriteCities { get; set; } = new List<UserWeatherFavorite>();
    }
}