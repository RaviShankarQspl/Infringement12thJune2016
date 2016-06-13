using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfringementCustomerWeb
{
    public partial class infringement
    {
        public string DisplayAmount { get; set; }
        public decimal ActualAmountToPay { get; set; }
        public string DisplayDueAmount { get; set; }
    }
}