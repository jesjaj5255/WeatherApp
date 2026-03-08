using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherApp.Models
{
    public class WeatherData
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Город обязателен")]
        [Display(Name = "Город")]
        [ForeignKey("City")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Температура обязательна")]
        [Range(-50, 60, ErrorMessage = "Температура должна быть от -50 до 60°C")]
        [Display(Name = "Температура (°C)")]
        public decimal Temperature { get; set; }

        [Required(ErrorMessage = "Влажность обязательна")]
        [Range(0, 100, ErrorMessage = "Влажность должна быть от 0 до 100%")]
        [Display(Name = "Влажность (%)")]
        public int Humidity { get; set; }

        [Required(ErrorMessage = "Скорость ветра обязательна")]
        [Range(0, 100, ErrorMessage = "Скорость ветра должна быть от 0 до 100 км/ч")]
        [Display(Name = "Скорость ветра (км/ч)")]
        public decimal WindSpeed { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Описание должно быть от 3 до 200 символов")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation
        [Display(Name = "Город")]
        public City? City { get; set; }
    }
}