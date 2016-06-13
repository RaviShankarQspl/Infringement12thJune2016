using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InfringementAPI.Resource
{
    [XmlRoot("Building")]
    [DataContract(Name = "Building")]
    public class BuildingResource
    {
        [DataMember(Name = "Id", IsRequired = true, Order = 1)]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember(Name = "Name", IsRequired = true, Order = 10)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [DataMember(Name = "Longitude", IsRequired = true, Order = 20)]
        [XmlElement("Longitude")]
        public string Longitude { get; set; }

        [DataMember(Name = "Latitude", IsRequired = true, Order = 30)]
        [XmlElement("Latitude")]
        public string Latitude { get; set; }

        [DataMember(Name = "CityId", IsRequired = true, Order = 40)]
        [XmlElement("CityId")]
        public int CityId { get; set; }

    }
}