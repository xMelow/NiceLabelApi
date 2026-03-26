using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
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
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var file = provider.Contents[0];
            var stream = await file.ReadAsStreamAsync();
            var variables = _labelService.GetVariables(stream);
            return Ok(variables);
        }
    }
}