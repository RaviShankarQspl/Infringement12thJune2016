
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class CarMakeModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        [Display(Name = "Make Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}