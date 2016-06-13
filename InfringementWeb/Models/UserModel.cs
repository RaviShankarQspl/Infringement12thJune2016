using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InfringementWeb
{

    [MetadataType(typeof(user.Metadata))]
    public partial class user
    {
        private sealed class Metadata
        {
            public int Id { get; set; }

            [Required]
            [StringLength(200, MinimumLength = 2)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Date Of Birth")]
            //[DataType(DataType.DateTime)]
            [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
            //[Remote("FutureDatecheck", "Users", HttpMethod = "POST", ErrorMessage = "User name already exists. Please enter a different user name.")]
            public Nullable<System.DateTime> DateOfBirth { get; set; }

            [Required]
            [StringLength(400, MinimumLength = 2)]
            [Display(Name = "Address 1")]
            public string Address1 { get; set; }

            [StringLength(400, MinimumLength = 2)]
            [Display(Name = "Address 2")]
            public string Address2 { get; set; }

            [StringLength(100, MinimumLength = 2)]
            [Display(Name = "Suburb")]
            public string Suburb { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 2)]
            [Display(Name = "City")]
            public string City { get; set; }

            [StringLength(45, MinimumLength = 2)]
            [Display(Name = "Home Phone")]
            [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
            public string HomePhone { get; set; }

            [StringLength(45, MinimumLength = 2)]
            [Display(Name = "Mobile Phone")]
            [Range(0, 999999999999999999, ErrorMessage = "Only Numbers allowed")]
            public string MobilePhone { get; set; }

            [Required]
            [StringLength(200, MinimumLength = 5)]
            [DataType(DataType.EmailAddress)]
            [EmailAddress]
            [Remote("doesUserNameExist", "Users", HttpMethod = "POST", ErrorMessage = "User name already exists. Please enter a different user name.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(200, MinimumLength = 1)]
            [Display(Name = "Job Title")]
            public string JobTitle { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [StringLength(20, ErrorMessage = "Must be between 5 and 20 characters", MinimumLength = 4)]
            [Display(Name = "Password")]
            [DataType(DataType.Password)]
            public string UserPassword { get; set; }
        }
    }
}