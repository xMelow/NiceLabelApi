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

            var labelContent = GetParameterContent(provider, "label");
            var printerNameContent = GetParameterContent(provider, "printerName");
            var quantityContent = GetParameterContent(provider, "quantity");

            if (labelContent == null) return BadRequest("Label needs to be present");
            if (quantityContent == null) return BadRequest("Print quantity needs to be present");
            
            var labelFile = await labelContent.ReadAsStreamAsync();
            var quantity = await quantityContent.ReadAsStringAsync();
            string printerName = null;
            
            if (printerNameContent != null)
                printerName = await printerNameContent.ReadAsStringAsync();
            
            if (!Int32.TryParse(quantity, out int parsedQuantity))
                return BadRequest("Quantity must be a valid number");
            
            try
            {
                _labelService.PrintLabel(labelFile, parsedQuantity, printerName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error printing label : {ex.Message}");
            }
            
            return Ok("Printing label...");
        }

        private HttpContent GetParameterContent(MultipartMemoryStreamProvider provider, string parameterName)
        {
            return provider.Contents
                    .FirstOrDefault(c => c.Headers.ContentDisposition?.Name?.Trim('"') == parameterName);
        }
    }
}