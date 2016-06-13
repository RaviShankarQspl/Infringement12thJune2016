using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace InfringementAPI.Resource
{
    [DataContract(Name = "Infringement")]
    [XmlRoot("Infringement")]
    public class InfringementResource
    {
        [DataMember(Name = "InfringementTime", Order = 10)]
        [XmlElement("InfringementTime", Order = 10, IsNullable = false)]
        public string InfringementTime { get; set; }

        [DataMember(Name = "InfringementNumber", Order = 20)]
        [XmlElement("InfringementTime", Order = 20, IsNullable = false)]
        public string InfringementNumber { get; set; }

        [DataMember(Name = "Rego", Order = 30)]
        [XmlElement("Rego", Order = 30, IsNullable = false)]
        public string Rego { get; set; }

        [DataMember(Name = "CityId")]
        [XmlElement("CityId", Order = 130, IsNullable = false)]
        public int CityId { get; set; }

        [DataMember(Name = "Building", Order = 40)]
        [XmlElement("Building", Order = 40, IsNullable = false)]
        public BuildingResource Building { get; set; }

        [DataMember(Name = "CarMake", Order = 50)]
        [XmlElement("CarMake", Order = 50, IsNullable = false)]
        public MakeResource CarMake { get; set; }

        [DataMember(Name = "CarModel", Order = 60)]
        [XmlElement("CarModel", Order = 60, IsNullable = false)]
        public ModelResource CarModel { get; set; }

        [DataMember(Name = "InfringementType", Order = 70)]
        [XmlElement("InfringementType", Order = 70, IsNullable = false)]
        public InfringementTypeResource InfringementType { get; set; }

        [DataMember(Name = "Amount", Order = 80)]
        [XmlElement("Amount", Order = 80, IsNullable = false)]
        public decimal Amount { get; set; }

        [DataMember(Name = "Comment", Order = 90)]
        [XmlElement("Comment", Order = 90, IsNullable = false)]
        public string Comment { get; set; }

        [DataMember(Name = "UserName", Order = 100)]
        [XmlElement("UserName", Order = 100, IsNullable = false)]
        public string UserName { get; set; }

        [DataMember(Name = "UploadTime", Order = 110)]
        [XmlElement("UploadTime", Order = 110, IsNullable = false)]
        public string UploadTime { get; set; }

        [DataMember(Name = "Latitude", Order = 120)]
        [XmlElement("Latitude", Order = 120, IsNullable = false)]
        public string Latitude { get; set; }

        [DataMember(Name = "Longitude", Order = 130)]
        [XmlElement("Longitude", Order = 130, IsNullable = false)]
        public string Longitude { get; set; }

        [DataMember(Name = "Pictures", Order = 140)]
        [XmlElement("Pictures", Order = 140, IsNullable = false)]
        public IList<string> Pictures { get; set; }

        [DataMember(Name = "OtherMake")]
        [XmlElement("OtherMake", Order = 130, IsNullable = false)]
        public string OtherMake { get; set; }

        [DataMember(Name = "OtherModel")]
        [XmlElement("OtherModel", Order = 130, IsNullable = false)]
        public string OtherModel { get; set; }

        [DataMember(Name = "Name")]
        [XmlElement("Name", Order = 130, IsNullable = false)]
        public string Name { get; set; }

        [DataMember(Name = "Street1")]
        [XmlElement("Street1", Order = 130, IsNullable = false)]
        public string Street1 { get; set; }

        [DataMember(Name = "Street2")]
        [XmlElement("Street2", Order = 130, IsNullable = false)]
        public string Street2 { get; set; }

        [DataMember(Name = "Suburb")]
        [XmlElement("Suburb", Order = 130, IsNullable = false)]
        public string Suburb { get; set; }

        [DataMember(Name = "CityName")]
        [XmlElement("CityName", Order = 130, IsNullable = false)]
        public string CityName { get; set; }

        [DataMember(Name = "PostCode")]
        [XmlElement("PostCode", Order = 130, IsNullable = false)]
        public string PostCode { get; set; }

        [DataMember(Name = "CountryId")]
        [XmlElement("CountryId", Order = 130, IsNullable = false)]
        public string CountryId { get; set; }

        [DataMember(Name = "Due Date")]
        [XmlElement("DueDate", Order = 130, IsNullable = false)]
        public DateTime DueDate { get; set; }

        [DataMember(Name = "AfterDueDate")]
        [XmlElement("AfterDueDate", Order = 130, IsNullable = false)]
        public decimal AfterDueDate { get; set; }

        [DataMember(Name = "OtherInfringementType")]
        [XmlElement("OtherInfringementType", Order = 130, IsNullable = false)]
        public string OtherInfringementType { get; set; }
    }
}