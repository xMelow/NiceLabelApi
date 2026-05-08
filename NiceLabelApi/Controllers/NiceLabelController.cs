using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using NiceLabelApi.Domain;
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
        [Route("labelPreview")]
        public async Task<IHttpActionResult> GetLabelPreview()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var width = await GetRequiredParameter<int>(provider, "width");
                var height = await GetRequiredParameter<int>(provider, "height");

                var labelPreviewBytes = _labelService.GetLabelPreview(label, width, height);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(labelPreviewBytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating label preview: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("labelPreviewBatch")]
        public async Task<IHttpActionResult> GetLabelPreviewBatch()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var variablesJson = await GetRequiredParameter<string>(provider, "variables");
                var variables = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(variablesJson);

                var labelPreviewBytes = _labelService.GetLabelPreviewBatch(label, variables);

                var base64Images = labelPreviewBytes.Select(b => Convert.ToBase64String(b)).ToList();
                
                return Ok(base64Images);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating label preview: {ex.Message}");
            }
        }
        
        [HttpPost]
        [Route("print")]
        public async Task<IHttpActionResult> PrintLabel()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var labelFile = await GetRequiredStream(provider, "label");
                var quantity = await GetRequiredParameter<int>(provider, "quantity");
                var printerName = await GetOptionalParameter(provider, "printerName");
                
                _labelService.PrintLabel(labelFile, quantity, printerName);
                
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

        [HttpPost]
        [Route("printLabelVariables")]
        public async Task<IHttpActionResult> PrintLabelVariables()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var variablesJson = await GetRequiredParameter<string>(provider, "variables");
                var printerName = await GetOptionalParameter(provider, "printerName");
                var variables = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(variablesJson);

                _labelService.PrintLabelVariables(label, variables, printerName);

                return Ok("Printing labels");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("printers")]
        public IHttpActionResult GetPrinters()
        {
            return Ok(_labelService.GetPrinters());
        }
        
        private async Task<T> GetRequiredParameter<T>(MultipartMemoryStreamProvider provider, string paramName)
        {
            var paramContent = GetParameterContent(provider, paramName);
            if (paramContent == null) throw new ValidationException($"{paramName} must be present");
            var paramString = await paramContent.ReadAsStringAsync();
            return (T)Convert.ChangeType(paramString, typeof(T));
        }
        
        private async Task<Stream> GetRequiredStream(MultipartMemoryStreamProvider provider, string paramName)
        {
            var paramContent = GetParameterContent(provider, paramName);
            if (paramContent == null) throw new ValidationException($"{paramName} must be present");
            return await paramContent.ReadAsStreamAsync();
        }
        
        private async Task<string> GetOptionalParameter(MultipartMemoryStreamProvider provider, string paramName)
        {
            var content = GetParameterContent(provider, paramName);
            if (content == null) return null;
            return await content.ReadAsStringAsync();
        }

        private HttpContent GetParameterContent(MultipartMemoryStreamProvider provider, string param)
        {
            return provider.Contents
                    .FirstOrDefault(c => c.Headers.ContentDisposition?.Name?.Trim('"') == param);
        }
    }
}