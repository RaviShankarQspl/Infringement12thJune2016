
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class InfringementPictureModel
    {
        [Required]
        [Display(Name = "Infringement Id")]
        public int InfringementId { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 2)]
        [Display(Name = "Type Of Infringement")]
        public string Description { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 2)]
        [Display(Name = "Type Of Infringement")]
        public string Location { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 2)]
        [Display(Name = "Type Of Infringement")]
        public string Longitude { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 2)]
        [Display(Name = "Type Of Infringement")]
        public string Latitude { get; set; }
    }
}