using InfringementAPI.Resource;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api")]
    public class BuildingController : ApiController
    {
        [HttpGet]
        [Route("buildings")]
        public IHttpActionResult GetBuildings()
        {
            var entities = new infringementEntities();

            var buildings = entities.parking_location
                .OrderBy(x => x.SortOrder)
                .ToList();

            return Ok(ResourceMapper.Map(buildings));
        }
    }
}
