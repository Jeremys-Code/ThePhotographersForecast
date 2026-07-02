using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThePhotographersForecast.Models
{
    public class WmoCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WmoCodeId { get; set; }
        public int WeatherCode { get; set; }
        public int IsDay { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
