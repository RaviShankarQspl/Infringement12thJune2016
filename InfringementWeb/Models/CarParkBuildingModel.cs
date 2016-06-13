
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class CarParkBuildingModel
    {
        public int Id { get; set; }

        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required]
        [StringLength(400, MinimumLength = 2)]
        [Display(Name = "Building Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(400, MinimumLength = 2)]
        [Display(Name = "Building Address")]
        public string Address { get; set; }        

        [StringLength(45, MinimumLength = 2)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        //[StringLength(45, MinimumLength = 2)]
        [Display(Name = "Longitude")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Length only 20 characters.")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed.")]
        public string Longitude { get; set; }

        //[StringLength(45, MinimumLength = 2)]
        [Display(Name = "Latitude")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Lengh only 20 characters.")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
        public string Latitude { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}