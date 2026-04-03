using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using NiceLabelApi.Domain;
using NiceLabelApi.Models;
using NiceLabelApi.Services;

namespace NiceLabelApi.Controllers
{
    [RoutePrefix("api/nicelabel")]
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
        [Route("print")]
        public async Task<IHttpActionResult> PrintLabel()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var printLabelRequest = await GetPrintLabelRequest(provider);
                _labelService.PrintLabel(printLabelRequest.LabelFile, printLabelRequest.Quantity, printLabelRequest.PrinterName);
                
                return Ok("Printing label...");
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        
        private async Task<PrintLabelRequest> GetPrintLabelRequest(MultipartMemoryStreamProvider provider)
        {
            PrintLabelRequest request = new PrintLabelRequest();
            
            var labelContent = GetParameterContent(provider, "label");
            var quantityContent = GetParameterContent(provider, "quantity");
            var printerNameContent = GetParameterContent(provider, "printerName");
            
            if (labelContent == null) throw new ValidationException("Label should be present");
            if (quantityContent == null) throw new ValidationException("Quantity should be present");
            
            request.LabelFile = await labelContent.ReadAsStreamAsync();
            var quantityString = await quantityContent.ReadAsStringAsync();
            
            if (!Int32.TryParse(quantityString, out int parsedQuantity)) throw new ValidationException("Quantity should be a valid number");
            if (printerNameContent != null) request.PrinterName = await printerNameContent.ReadAsStringAsync();
            
            request.Quantity = parsedQuantity;
            return request;
        }

        private HttpContent GetParameterContent(MultipartMemoryStreamProvider provider, string param)
        {
            return provider.Contents
                    .FirstOrDefault(c => c.Headers.ContentDisposition?.Name?.Trim('"') == param);
        }
    }
}