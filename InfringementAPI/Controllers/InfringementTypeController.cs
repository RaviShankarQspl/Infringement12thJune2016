using InfringementAPI.Resource;
using System.Linq;
using System.Web.Http;

namespace InfringementAPI.Controllers
{
    [RoutePrefix("api")]
    public class InfringementTypeController : ApiController
    {
        [HttpGet]
        [Route("infringementTypes")]
        public IHttpActionResult GetCarMakes()
        {
            var entities = new infringementEntities();

            var infringementTypes = entities.infringementtypes
                .OrderBy(x => x.SortOrder)
                .ToList();

            return Ok(ResourceMapper.Map(infringementTypes));
        }
    }
}
