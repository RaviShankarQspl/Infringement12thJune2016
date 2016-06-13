using InfringementAPI.Resource;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api")]
    public class CarMakeController : ApiController
    {
        [HttpGet]
        [Route("carMakes")]
        public IHttpActionResult GetCarMakes()
        {
            var entities = new infringementEntities();

            var carMakes = entities.makes
                .OrderBy(x => x.SortOrder)
                .ToList();

            return Ok(ResourceMapper.Map(carMakes));
        }
    }
}
