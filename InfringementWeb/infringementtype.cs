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
    
    public partial class infringementtype
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public infringementtype()
        {
            this.infringements = new HashSet<infringement>();
        }
    
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public Nullable<int> SortOrder { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<infringement> infringements { get; set; }
    }
}
