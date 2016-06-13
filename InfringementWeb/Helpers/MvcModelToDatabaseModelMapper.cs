using InfringementWeb.Models;
using System;
using System.Globalization;

namespace InfringementWeb.Helpers
{
    public static class MvcModelToDatabaseModelMapper
    {
        public static city MapCityForCreate(CityModel model)
        {
            infringementEntities entity = new infringementEntities();

            var result = new city();
            MapCityForEdit(model, result);
            return result;
        }

        public static void MapCityForEdit(CityModel model, city cityEntity)
        {
            cityEntity.name = model.Name;
            cityEntity.SortOrder = model.SortOrder;
        }

        public static CityModel MapCityForDisplay(city model)
        {
            infringementEntities entity = new infringementEntities();

            return new CityModel { Id = model.id, Name = model.name, SortOrder = model.SortOrder ?? 0 };
        }

        public static parking_location MapBuildingForCreate(CarParkBuildingModel model)
        {
            var entity = new parking_location();
            MapBuildingForEdit(model, entity);
            return entity;
        }

        public static CarParkBuildingModel MapBuildingForDisplay(parking_location model)
        {
            infringementEntities entity = new infringementEntities();

            return new CarParkBuildingModel {
                Id = model.Id,
                Name = model.Name,
                SortOrder = model.SortOrder ?? 0,
                Address = model.Address,
                CityId = model.CityId,
                Description = model.Description,
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };
        }

        public static void MapBuildingForEdit(CarParkBuildingModel model, parking_location entity)
        {
            entity.Name = model.Name;
            entity.SortOrder = model.SortOrder;
            entity.Address = model.Address;
            entity.CityId = model.CityId;
            entity.Description = model.Description;
            if (model.Latitude != null && model.Latitude != "")
            {
                entity.Latitude = model.Latitude.ToString().Trim();
            }
            else
            {
                entity.Latitude = model.Latitude;
            }
            if (model.Longitude != null && model.Longitude != "")
            {
                entity.Longitude = model.Longitude.ToString().Trim();
            }
            else
            {
                entity.Longitude = model.Longitude;
            }
        }

        public static infringementtype MapInfringementTypeForCreate(InfringementTypeModel model)
        {
            var entity = new infringementtype();
            MapInfringementTypeForEdit(model, entity);
            return entity;
        }

        public static InfringementTypeModel MapInfringementTypeForDisplay(infringementtype model)
        {
            return new InfringementTypeModel
            {
                Id = model.Id,
                Type = model.Type.Trim(),
                Amount = model.Amount,
                SortOrder = model.SortOrder ?? 0
            };
        }

        public static void MapInfringementTypeForEdit(InfringementTypeModel model, infringementtype entity)
        {
            entity.Type = model.Type;
            entity.SortOrder = model.SortOrder;
            entity.Amount = model.Amount;
        }

        public static make MapCarMakeForCreate(CarMakeModel model)
        {
            var entity = new make();
            MapCarMakeForEdit(model, entity);
            return entity;
        }

        public static CarMakeModel MapCarMakeForDisplay(make model)
        {
            return new CarMakeModel
            {
                Id = model.id,
                Name = model.Name.Trim(),
                SortOrder = model.SortOrder ?? 0
            };
        }

        public static void MapCarMakeForEdit(CarMakeModel model, make entity)
        {
            entity.Name = model.Name.Trim();
            entity.SortOrder = model.SortOrder;
        }

        public static carmodel MapCarModelForCreate(ModelForCarMakeModel model)
        {
            var entity = new carmodel();
            MapCarModelForEdit(model, entity);
            return entity;
        }

        public static ModelForCarMakeModel MapCarModelForDisplay(carmodel model)
        {
            return new ModelForCarMakeModel
            {
                Id = model.Id,
                MakeId = model.MakeId,
                Name = model.Name.Trim(),
                SortOrder = model.SortOrder ?? 0
            };
        }

        public static void MapCarModelForEdit(ModelForCarMakeModel model, carmodel entity)
        {
            if (model.Name != null)
            {
                entity.Name = model.Name.Trim();
            }
            else
            {
                entity.Name = model.Name;
            }
            entity.SortOrder = model.SortOrder;
            entity.MakeId = model.MakeId;
        }

        public static infringement MapInfringementForCreate(InfringementModel model)
        {
            var entity = new infringement();

            var uploadTime = DateTime.Now;
            entity.UploadTime = uploadTime;
            model.StatusId = (int)InfringementStatus.Pending;
            if (model.Number == null)
            {
                entity.Number = InfringementNumberGenerator.Generate(model.IncidentTime, model.User);
            }
            else
                entity.Number = model.Number;


            //string dateString = model.IncidentTime.ToString("yyyy-MM-dd hh:mm");

            //DateTime newdate = Convert.ToDateTime(dateString);
            //DateTime newdate1 = DateTime.Parse(dateString);
            //model.IncidentTime = newdate1;

            //DateTime dt = new DateTime();
            //DateTime dt1 = new DateTime(model.IncidentTime.Year, model.IncidentTime.Month, model.IncidentTime.Day, model.IncidentTime.Hour, model.IncidentTime.Minute, 0);
            //string[] startdatetime = dateString.ToString().Split(' ');
            //if (startdatetime.Length > 0)
            //{
            //    string[] startdate = startdatetime[0].Split('-');
            //    string[] starttime = startdatetime[1].Split(':');
            //    dt = new DateTime(Convert.ToInt32(startdate[0]), Convert.ToInt32(startdate[1]), Convert.ToInt32(startdate[2]), Convert.ToInt32(starttime[0]), Convert.ToInt32(starttime[1]), Convert.ToInt32(starttime[2]));
            //    model.IncidentTime = dt1;
            //    //entity.IncidentTime = dt;
            //}

            entity.User = model.User;
            MapInfringementForEdit(model, entity);
            return entity;
        }

        public static InfringementModel MapInfringementForDisplay(infringement model)
        {
            return new InfringementModel
            {
                Amount = model.Amount,
                Comment = model.Comment,
                IncidentTime = model.IncidentTime,
                InfringementTypeId = model.InfringementTypeId,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                MakeId = model.MakeId ?? 0,
                ModelId = model.ModelId ?? 0,
                OtherMake = model.OtherMake,
                OtherModel = model.OtherModel,
                Number = model.Number,
                ParkingLocationId = model.ParkingLocationId,
                Rego = model.Rego,
                User = model.User,
                StatusId = model.StatusId,

               DueDate = model.DueDate.Value,
               AfterDueDate = model.AfterDueDate.Value,

               Name = model.OwnerName,
               Street1 = model.Street1,
               Street2 = model.Street2,
               Suburb = model.Suburb,
               PostCode = model.PostCode,
               CountryId = model.CountryId,
               CityName = model.CityName,
                OtherInfringementType = model.OtherInfringementType
            };
        }

        public static infringement MapInfringementForDisplayDelete(infringement model)
        {

            return new infringement
            {
                Amount = model.Amount,
                Comment = model.Comment,
                IncidentTime = model.IncidentTime,
                InfringementTypeId = model.InfringementTypeId,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                MakeId = model.MakeId ?? 0,
                ModelId = model.ModelId ?? 0,
                OtherMake = model.OtherMake,
                OtherModel = model.OtherModel,
                Number = model.Number,
                ParkingLocationId = model.ParkingLocationId,
                Rego = model.Rego,
                User = model.User,
                StatusId = model.StatusId,

               
                DueDate = model.DueDate,
                AfterDueDate = model.AfterDueDate,

                OwnerName = model.OwnerName,
                Street1 = model.Street1,
                Suburb = model.Suburb,
                PostCode = model.PostCode,
                CountryId = model.CountryId,
                CityName = model.CityName,
                OtherInfringementType = model.OtherInfringementType

        };
        }

        public static void MapInfringementForEdit(InfringementModel model, infringement entity)
        {
            entity.Amount = model.Amount;
            if (entity.Comment != null && entity.Comment != "")
            {
                entity.Comment = model.Comment.Trim();
            }
            else
            {
                entity.Comment = model.Comment;
            }
            entity.IncidentTime = model.IncidentTime;
            entity.InfringementTypeId = model.InfringementTypeId;
            if (model.Latitude != null && model.Latitude != "")
            {
                entity.Latitude = model.Latitude.Trim();
            }
            else
            {
                entity.Latitude = model.Latitude;
            }
            if (model.Longitude != null && model.Longitude != "")
            {
                entity.Longitude = model.Longitude.Trim();
            }
            else
            {
                entity.Longitude = model.Longitude;
            }
            entity.MakeId = model.MakeId;
            //entity.Model = model.CarModel;
            entity.ModelId = model.ModelId;
            entity.OtherModel = model.OtherModel;
            entity.OtherMake = model.OtherMake;
            entity.ParkingLocationId = model.ParkingLocationId;
            entity.Rego = model.Rego;
            entity.StatusId = model.StatusId;

            entity.OtherMake = model.OtherMake;
            entity.OtherModel = model.OtherModel;
            entity.DueDate = model.IncidentTime.AddDays(21);
            entity.AfterDueDate = entity.Amount + 20;

            entity.OwnerName = model.Name;
            entity.Street1 = model.Street1;
            entity.Street2 = model.Street2;
            entity.Suburb = model.Suburb;
            entity.PostCode = model.PostCode;
            entity.CountryId = model.CountryId;
            entity.CityName = model.CityName;
            entity.OtherInfringementType = model.OtherInfringementType;
            entity.User = model.User;
           
            
        }
    }
}