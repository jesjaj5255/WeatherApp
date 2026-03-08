using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название города обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        [Display(Name = "Название города")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Страна обязательна")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Страна должна быть от 2 до 100 символов")]
        [Display(Name = "Страна")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Широта обязательна")]
        [Range(-90, 90, ErrorMessage = "Широта должна быть от -90 до 90")]
        [Display(Name = "Широта")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Долгота обязательна")]
        [Range(-180, 180, ErrorMessage = "Долгота должна быть от -180 до 180")]
        [Display(Name = "Долгота")]
        public decimal Longitude { get; set; }

        [StringLength(500, ErrorMessage = "Описание не может быть больше 500 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        [Display(Name = "История погоды")]
        public ICollection<WeatherData> WeatherHistory { get; set; } = new List<WeatherData>();
    }
}