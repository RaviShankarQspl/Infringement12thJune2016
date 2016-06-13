using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfringementAPI.Resource
{
    public static class ResourceMapper
    {
        public static IList<MakeResource> Map(IList<make> carMakes)
        {
            return carMakes.Select(x => new MakeResource { Id = x.id, Name = x.Name }).ToList();
        }

        private static MakeResource Map(make carMake)
        {
            return new MakeResource { Id = carMake.id, Name = carMake.Name };
        }

        public static IList<ModelResource> Map(IList<carmodel> models)
        {
            return models.Select(x => new ModelResource {
                Name = x.Name,
                Id = x.MakeId,
                MakeName = x.make.Name,
                MakeId = x.make.id
            }).ToList();
        }

        public static IList<CityResource> Map(IList<city> cities)
        {
            return cities.Select(x => new CityResource { Id = x.id, Name = x.name }).ToList();
        }

        public static ModelResource Map(int modelid)
        {
            var entity = new infringementEntities();
            ModelResource model = (from m in entity.carmodels
                                   where m.Id == modelid
                                   select new ModelResource {Id = m.Id, Name=m.Name}).FirstOrDefault();

            return model;
        }

        public static InfringementResource Map(infringement infringement)
        {
            return new InfringementResource
            {
                Amount = infringement.Amount,
                Building = Map(infringement.parking_location),
                CarMake = Map(infringement.make),
                CarModel = Map(infringement.ModelId.Value),
                Comment = infringement.Comment,
                InfringementNumber = infringement.Number,
                InfringementTime = infringement.IncidentTime.ToString("yyyy-MM-dd H:mm:ss zzz"),
                InfringementType = Map(infringement.infringementtype),
                Latitude = infringement.Latitude,
                Longitude = infringement.Longitude,
                Rego = infringement.Rego,
                UserName = infringement.User,
                UploadTime = infringement.UploadTime.Value.ToString("yyyy-MM-dd H:mm:ss zzz"),

                OtherMake = infringement.OtherMake,
                OtherModel = infringement.OtherModel,
                DueDate = infringement.DueDate.Value,
                AfterDueDate = infringement.AfterDueDate.Value,

                Name = infringement.OwnerName,
                Street1 = infringement.Street1,
                Street2 = infringement.Street2,
                Suburb = infringement.Suburb,
                PostCode = infringement.PostCode,
                CountryId = infringement.CountryId,
                CityName = infringement.CityName,
                OtherInfringementType = infringement.OtherInfringementType,
               // CityId = infringement.CityId.Value
            };
        }

        public static BuildingResource Map(parking_location location)
        {
            return new BuildingResource
            {
                Id = location.Id,
                Name = location.Name,
                Longitude = location.Longitude,
                Latitude = location.Latitude
            };
        }

        public static IList<BuildingResource> Map(IList<parking_location> locations)
        {
            return locations.Select(x => new BuildingResource { Id = x.Id, CityId = x.CityId, Name = x.Name, Longitude = x.Longitude, Latitude = x.Latitude }).ToList();
        }

        public static IList<InfringementTypeResource> Map(IList<infringementtype> infringementTypes)
        {
            return infringementTypes.Select(x => 
            new InfringementTypeResource {
                Id = x.Id,
                Type = x.Type,
                Amount = x.Amount
            }).ToList();
        }

        private static InfringementTypeResource Map(infringementtype infringementType)
        {
            return 
            new InfringementTypeResource
            {
                Id = infringementType.Id,
                Type = infringementType.Type,
                Amount = infringementType.Amount
            };
        }
    }
}