using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfringementWeb
{
    public partial class infringement
    {
        public string ImagePath { get; set; }
        public string InfringementStatus { get; set; }

        public string DisplayAmount { get; set; }
        public decimal ActualAmountToPay { get; set; }
        public string DisplayDueAmount { get; set; }


        public string ImageLatitude { get; set; }
        public string ImageLongitude { get; set; }
    }
}