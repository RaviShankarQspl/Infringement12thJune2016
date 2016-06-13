using InfringementAPI.Resource;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api")]
    public class CarModelController : ApiController
    {
        [HttpGet]
        [Route("carModels")]
        public IHttpActionResult GetCarModels()
        {
            var entities = new infringementEntities();

            var carModels = entities.carmodels
                .OrderBy(x => x.SortOrder)
                .ToList();

            return Ok(ResourceMapper.Map(carModels));
        }
    }
}
