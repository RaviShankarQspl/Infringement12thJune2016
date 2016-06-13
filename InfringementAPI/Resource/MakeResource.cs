using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InfringementAPI.Resource
{
    [XmlRoot("Make")]
    [DataContract(Name = "Make")]
    public class MakeResource
    {
        [DataMember(Name = "Id", IsRequired = true, Order = 1)]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember(Name = "Name", IsRequired = true, Order = 10)]
        [XmlElement("Name")]
        public string Name { get; set; }

    }
}