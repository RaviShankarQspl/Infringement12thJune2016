using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class ModelForCarMakeModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Make")]
        public int MakeId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "Model Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

    }
}