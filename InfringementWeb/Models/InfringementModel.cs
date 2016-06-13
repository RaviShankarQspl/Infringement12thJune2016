using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
namespace InfringementWeb.Models
{
    public enum InfringementStatus
    {
        Pending = 1,
        Paid = 2,
        Objected = 3,
        Cancelled = 4
    }

    public class InfringementModel
    {
        [Display(Name = "Incident Time")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy HH:mm}")]
        public DateTime IncidentTime { get; set; }

        [StringLength(8, MinimumLength = 2)]
        //[Remote("IsAlphaNumeric", "Infringements", HttpMethod = "POST", ErrorMessage = "Use only Alphabets and Numbers only.")]
        [Display(Name = "Infringement Number")]
        public string Number { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Use only Alphabets and Numbers only")]
        [Display(Name = "Car Registration")]
        public string Rego { get; set; }

        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required]
        [Display(Name = "Car Park Building")]
        public int ParkingLocationId { get; set; }

        [Display(Name = "Other Car Make")]
        public string OtherMake { get; set; }

        [Display(Name = "Other Car Model")]
        public string OtherModel { get; set; }

        [Required]  
        [Display(Name = "Car Make")]
        public int? MakeId { get; set; }

        [Required]
        [Display(Name = "Car Model")]
        public int? ModelId { get; set; }

        //[Required]
        //[Display(Name = "Car Model")]
        //[StringLength(200, MinimumLength = 2)]
        //public string CarModel { get; set; }
         
        [Required]
        [Display(Name = "Infringement Type")]
        public int InfringementTypeId { get; set; }

        [Display(Name = "Other Infringement Type")]
        public string OtherInfringementType { get; set; }

        [Required]
        [Range(1, 999999999999999999, ErrorMessage = "Only Numbers allowed with greater than zero.")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        //[Required]
        [Display(Name = "Comment")]
        [StringLength(4000, MinimumLength = 1)]
        public string Comment { get; set; }

        //[Required]
        [Display(Name = "OfficerCode")]
        //[StringLength(5, MinimumLength = 1)]
        public string User { get; set; }

        //[Required]
        [Display(Name = "Latitude")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Lengh only 20 characters.")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
        public string Latitude { get; set; }

        //[Required]
        [Display(Name = "Longitude")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Length only 20 characters.")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed.")]
        public string Longitude { get; set; }

        [Required]
        [Display(Name = "Infringement Status")]
        public int StatusId { get; set; }

        
        [Display(Name = "Other Make/Model")]
        public bool IsOtherMake { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Street1")]
        public string Street1 { get; set; }

        [Display(Name = "Street2")]
        public string Street2 { get; set; }

        [Display(Name = "Suburb")]
        public string Suburb { get; set; }

        [Display(Name = "City ")]
        public string CityName { get; set; }

        [Display(Name = "Post Code")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed.")]
        public string PostCode { get; set; }

        [Display(Name = "Country")]
        public string CountryId { get; set; }

        [Display(Name = "Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime DueDate { get; set; }

        [Display(Name = "After Due Date")]
        public decimal AfterDueDate { get; set; }

        [Display(Name = "ImageLatitude")]
        [StringLength(20, MinimumLength = 1)]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
        public string ImageLatitude { get; set; }

        [Display(Name = "ImageLongitude")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Length only 20 characters.")]
        [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
        public string ImageLongitude { get; set; }

        [Display(Name = "ImageComment")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Max Length only 20 characters.")]
        [RegularExpression(@"^[a-zA-Z]+[ a-zA-Z-_]*$", ErrorMessage = "Use only alphabets only")]
        public string ImageComment { get; set; }

    }
}