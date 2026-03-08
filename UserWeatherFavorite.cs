using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Models
{
    public class UserWeatherFavorite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("City")]
        public int CityId { get; set; }

        [Display(Name = "Дата добавления")]
        public DateTime AddedDate { get; set; } = DateTime.Now;

        // Navigation
        public User? User { get; set; }
        public City? City { get; set; }
    }
}