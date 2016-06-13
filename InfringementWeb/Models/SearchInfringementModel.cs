using System;
using System.ComponentModel.DataAnnotations;

namespace InfringementWeb.Models
{
    public class SearchInfringementModel
    {
        [Display(Name = "Enter Car Reg No")]
        public string SearchOnRegoNumber { get; set; }

        [Display(Name = "Enter Infringement Number")]
        public string SearchString { get; set; }
    }
}