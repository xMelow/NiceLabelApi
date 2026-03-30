using System;
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
        
        [HttpPost]
        [Route("variables")]
        public async Task<IHttpActionResult> Variables()
        {
            var labelFileStream = await Request.Content.ReadAsStreamAsync();

            if (labelFileStream.Length == 0) return BadRequest("Body can't be empty");
            
            var variables = _labelService.GetVariables(labelFileStream);
            return Ok(variables);
        }
        
        [HttpPost]
        [Route("printLabel")]
        public async Task<IHttpActionResult> PrintLabel()
        {
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var labelContent = provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition?.Name?.Trim('"') == "label");
            var printerIpContent = provider.Contents
                .FirstOrDefault(c => c.Headers.ContentDisposition?.Name?.Trim('"') == "printerIp");

            if (labelContent == null) return BadRequest("Label needs to be present");
            var labelFile = await labelContent.ReadAsStreamAsync();
            string printerIp = null;
            
            if (printerIpContent != null)
                printerIp = await printerIpContent.ReadAsStringAsync();
            
            try
            {
                _labelService.PrintLabel(labelFile, printerIp);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error printing label : {ex.Message}");
            }
            
            return Ok("Printing label...");
        }
    }
}