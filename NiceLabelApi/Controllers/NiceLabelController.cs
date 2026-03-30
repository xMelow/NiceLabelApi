using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using NiceLabelApi.Domain;
using NiceLabelApi.Services;

namespace NiceLabelApi.Controllers
{
    [RoutePrefix("nicelabel")]
    public class NiceLabelController : ApiController
    {
        private readonly INiceLabelService _labelService;

        public NiceLabelController()
        {
            _labelService = new NiceLabelService(new NiceLabelEngine());
        }
        
        [HttpGet]
        [Route("test")]
        public IHttpActionResult Test()
        {
            return Ok("NiceLabel API is running!");
        }
        
        [HttpPost]
        [Route("variables")]
        public async Task<IHttpActionResult> Variables()
        {
            var labelFileStream = await Request.Content.ReadAsStreamAsync();

            if (labelFileStream.Length == 0) return BadRequest("Body can't be empty");
            
            var variables = _labelService.GetVariables(labelFileStream);
            return Ok(variables);
        }
    }
}