
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class CityModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "City Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}