using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InfringementAPI.Resource
{
    [XmlRoot("Model")]
    [DataContract(Name = "Model")]
    public class ModelResource
    {
        [DataMember(Name = "Name", IsRequired = true, Order = 10)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [DataMember(Name = "Id", IsRequired = true, Order = 20)]
        [XmlElement("Id")]
        public int Id { get; set; }

        [DataMember(Name = "MakeId", IsRequired = true, Order = 20)]
        [XmlElement("MakeId")]
        public int MakeId { get; set; }



        [DataMember(Name = "MakeName", IsRequired = true, Order = 30)]
        [XmlElement("MakeName")]
        public string MakeName { get; set; }

    }
}