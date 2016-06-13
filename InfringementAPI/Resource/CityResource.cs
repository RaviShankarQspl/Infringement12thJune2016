using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace InfringementAPI.Resource
{
    [XmlRoot("City")]
    [DataContract(Name = "City")]
    public class CityResource
    {
        [DataMember(Name = "Id", IsRequired = true, Order = 1)]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember(Name = "Name", IsRequired = true, Order = 10)]
        [XmlElement("Name")]
        public string Name { get; set; }

    }
}