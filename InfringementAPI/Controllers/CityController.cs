using InfringementAPI.Resource;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api")]
    public class CityController : ApiController
    {
        [HttpGet]
        [Route("cities")]
        public IHttpActionResult GetCities()
        {
            var entities = new infringementEntities();

            //var cities = entities.cities
            //    .OrderBy(x => x.SortOrder)
            //    .ToList();

            var cities = entities.cities.ToList();

            return Ok(ResourceMapper.Map(cities));
       } 
    }
}
