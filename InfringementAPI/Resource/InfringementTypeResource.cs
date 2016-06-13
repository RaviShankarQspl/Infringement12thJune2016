using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InfringementAPI.Resource
{
    [XmlRoot("InfringementType")]
    [DataContract(Name = "InfringementType")]
    public class InfringementTypeResource
    {
        [XmlElement("Id")]
        [DataMember(Name = "Id", IsRequired = true, Order = 1)]
        public int Id { get; set; }

        [XmlElement("Type")]
        [DataMember(Name = "Type", IsRequired = true, Order = 10)]
        public string Type { get; set; }

        [XmlElement("Amount")]
        [DataMember(Name = "Amount", IsRequired = true, Order = 20)]
        public decimal Amount { get; set; }
    }
}