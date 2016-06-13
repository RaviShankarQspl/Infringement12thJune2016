using System;

namespace InfringementAPI.Request
{
    public enum InfringementStatus
    {
        Pending = 1,
        Paid = 2,
        Objected = 3,
        Cancelled = 4
    }
    public static class RequestToEntityMapper
    {
        public static infringement Map(InfringementRequest request)
        {
            infringement Infringement = new infringement();

            DateTime dt = new DateTime();

            string[] startdatetime = request.InfringementTime.Split(' ');
            if (startdatetime.Length > 0)
            {
                string[] startdate = startdatetime[0].Split('-');
                string[] starttime = startdatetime[1].Split(':');
                dt = new DateTime(Convert.ToInt32(startdate[0]), Convert.ToInt32(startdate[1]), Convert.ToInt32(startdate[2]), Convert.ToInt32(starttime[0]), Convert.ToInt32(starttime[1]), Convert.ToInt32(starttime[2]));
            }

            DateTime d = Convert.ToDateTime(request.InfringementTime);

                Infringement.Amount = request.Amount;
                Infringement.Comment = request.Comment;
                Infringement.IncidentTime = dt; // DateTime.ParseExact(request.InfringementTime, "dd/MM/yyyy HH:mm", null);
                Infringement.InfringementTypeId = request.InfringementTypeId;
                Infringement.Latitude = request.Latitude;
                Infringement.Longitude = request.Longitude;
                Infringement.MakeId = request.CarMakeId;
                Infringement.Model = request.CarModel;
                Infringement.Number = request.InfringementNumber;
                Infringement.ParkingLocationId = request.BuildingId;
                Infringement.Rego = request.Rego;
                Infringement.UploadTime = DateTime.Now;
                Infringement.User = request.UserName;
                Infringement.StatusId = (int)InfringementStatus.Pending;

                Infringement.OtherMake =request.OtherMake;
                Infringement.OtherModel =request.OtherModel;
                Infringement.DueDate = dt.AddDays(21);
                Infringement.AfterDueDate = request.Amount + 20;
                
                Infringement.OwnerName =request.Name;
                Infringement.Street1 =request.Street1;
                Infringement.Street2 =request.Street2;
                Infringement.Suburb =request.Suburb;
                Infringement.PostCode =request.PostCode;
                Infringement.CountryId =request.CountryId;
                Infringement.CityName =request.CityName;
                Infringement.OtherInfringementType = request.OtherInfringementType;
                Infringement.Pay = false;

            return Infringement;
        }

        public static void Map(InfringementRequest request, infringement infringement)
        {
            DateTime dt = new DateTime();

            string[] startdatetime = request.InfringementTime.Split(' ');
            if (startdatetime.Length > 0)
            {
                string[] startdate = startdatetime[0].Split('-');
                string[] starttime = startdatetime[1].Split(':');
                dt = new DateTime(Convert.ToInt32(startdate[0]), Convert.ToInt32(startdate[1]), Convert.ToInt32(startdate[2]), Convert.ToInt32(starttime[0]), Convert.ToInt32(starttime[1]), Convert.ToInt32(starttime[2]));
            }

            infringement.Amount = request.Amount;
            infringement.Comment = request.Comment;
            infringement.IncidentTime = dt; // DateTime.ParseExact(request.InfringementTime, "dd/MM/yyyy HH:mm", null);
            infringement.InfringementTypeId = request.InfringementTypeId;
            infringement.Latitude = request.Latitude;
            infringement.Longitude = request.Longitude;
            infringement.MakeId = request.CarMakeId;
            infringement.Model = request.CarModel;
            infringement.Number = request.InfringementNumber;
            infringement.ParkingLocationId = request.BuildingId;
            infringement.Rego = request.Rego;
            infringement.UploadTime = DateTime.Now;
            infringement.User = request.UserName;

            infringement.StatusId = (int)InfringementStatus.Pending;

            infringement.OtherMake = request.OtherMake;
            infringement.OtherModel = request.OtherModel;
            infringement.DueDate = dt.AddDays(21);
            infringement.AfterDueDate = request.Amount + 20;

            infringement.OwnerName = request.Name;
            infringement.Street1 = request.Street1;
            infringement.Street2 = request.Street2;
            infringement.Suburb = request.Suburb;
            infringement.PostCode = request.PostCode;
            infringement.CountryId = request.CountryId;
            infringement.CityName = request.CityName;
            infringement.OtherInfringementType = request.OtherInfringementType;
            infringement.Pay = false;
        }
    }
}