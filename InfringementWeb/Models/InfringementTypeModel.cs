
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class InfringementTypeModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(4000, MinimumLength = 2)]
        [Display(Name = "Type Of Infringement")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Amount For Infringement")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}