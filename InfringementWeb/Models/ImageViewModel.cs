using System.ComponentModel.DataAnnotations;
using System.Web;

public class ImageViewModel
{
    [Required]
    [StringLength(8, MinimumLength = 6)]
    public string InfringementNumber { get; set; }

    [DataType(DataType.Upload)]
    public HttpPostedFileBase ImageUpload { get; set; }
}