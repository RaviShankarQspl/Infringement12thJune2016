using InfringementAPI.Request;
using InfringementAPI.Resource;
using System;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api/infringement")]
    public class InfringementController : ApiController
    {
        [HttpPost]
        [Route("create")]
        public IHttpActionResult Post(InfringementRequest request)
        {
            try
            {
                var entity = new infringementEntities();

                user lUser = (from a in entity.users
                             where a.Email == request.LoginId && a.UserPassword == request.Password && a.IsActive == true
                              select a).FirstOrDefault();

                if (lUser != null)
                {

                    if (entity.infringements.FirstOrDefault(x => x.Number == request.InfringementNumber) != null)
                    {
                        //return BadRequest("Infringement already exists");
                        var error = "Infringement already exists with InfringementNumber:" + request.InfringementNumber;
                        return BadRequest(error);
                    }
                    int infringementCity = entity.parking_location.FirstOrDefault(x => x.Id == request.BuildingId).CityId;
                    
                    var carmodel1 = entity.carmodels.Where(x => x.Name == request.CarModel).FirstOrDefault();
                    

                    infringement dbInfringement = RequestToEntityMapper.Map(request);
                    if (carmodel1 != null)
                    {
                        dbInfringement.ModelId = Convert.ToInt16(carmodel1.Id);
                    }

                    dbInfringement.CityId = infringementCity;
                    dbInfringement.CreatedBy = lUser.Id;
                    dbInfringement.CreatedDate = System.DateTime.Now;
                    dbInfringement.Pay = false;
                    dbInfringement.GeneratedFrom = 2;
                    entity.infringements.Add(dbInfringement);
                    entity.SaveChanges();

                    return GetByInfringementNumber(request.InfringementNumber);
                }
                else
                {
                    var error = "Un-Authhorized access.  Please check your User details.";
                    return Ok(error);
                }
            }

            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                string errormsg = "";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        errormsg += validationError.PropertyName + " : " + validationError.ErrorMessage + ", ";
                    }
                }
                return Ok(errormsg);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("update")]
        public IHttpActionResult Put(InfringementRequest request)
        {
            try
            {
                var entity = new infringementEntities();
                var infringement = entity.infringements.FirstOrDefault(x => x.Number == request.InfringementNumber);

                if (infringement == null)
                    return NotFound();

                RequestToEntityMapper.Map(request, infringement);
                entity.SaveChanges();
                return GetByInfringementNumber(infringement.Number);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("infringementNumber/{infringementNumber}")]
        public IHttpActionResult GetByInfringementNumber(string infringementNumber)
        {
            try
            {
                var entity = new infringementEntities();
                var infringement = entity.infringements.FirstOrDefault(x => x.Number == infringementNumber);

                if (infringement == null)
                    return NotFound();

                var resource = ResourceMapper.Map(infringement);

                return Ok(resource);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("rego/{rego}")]
        public IHttpActionResult GetByRego(string rego)
        {
            try
            {
                var entity = new infringementEntities();
                var infringement = entity.infringements
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault(x => x.Rego == rego & x.StatusId == 1);

                if (infringement == null)
                    return NotFound();

                var resource = ResourceMapper.Map(infringement);

                return Ok(resource);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
