//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InfringementWeb
{
    using System;
    using System.Collections.Generic;
    
    public partial class user
    {
       
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }

        //public Nullable<DateTime> DateOfBirth { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string JobTitle { get; set; }
        public string PhotoLocation { get; set; }
        public Nullable<int> UserType { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string UserPassword { get; set; }
    }
}
