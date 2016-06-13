using System;
using System.ComponentModel.DataAnnotations;

namespace InfringementCustomerWeb.Models
{
    public class SearchInfringementModel
    {
        [Display(Name = "Enter Car Reg No")]
        public string SearchOnRegoNumber { get; set; }

        [Display(Name = "Enter Infringement Number")]
        public string SearchString { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}