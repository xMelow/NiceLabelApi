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
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            var labelFileStream = await GetRequiredStream(provider, "label");

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
        [Route("printLabel")]
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
        [Route("printVariableLabel")]
        public async Task<IHttpActionResult> PrintVariableLabel()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var variablesJson = await GetRequiredParameter<string>(provider, "variables");
                var printerName = await GetOptionalParameter(provider, "printerName");
                var variables = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(variablesJson);
                var amount = await GetRequiredParameter<int>(provider, "amount");

                _labelService.PrintVariableLabel(label, variables, printerName, amount);

                return Ok("Printing labels");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost]
        [Route("printLabelWithSettings")]
        public async Task<IHttpActionResult> PrintLabelWithSettings()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var printSettingsJson = await GetRequiredParameter<string>(provider, "printSettings");
                var labelVariableJson = await GetRequiredParameter<string>(provider, "labelVariables");
                var printerName = await GetOptionalParameter(provider, "printerName");
                var printSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(printSettingsJson);
                var labelVariables = JsonConvert.DeserializeObject<Dictionary<string, string>>(labelVariableJson);

                _labelService.PrintLabelWithSettings(label, labelVariables, printSettings, printerName);

                return Ok("Printing labels");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        
        [HttpPost]
        [Route("printToPdf")]
        public async Task<IHttpActionResult> PrintToPdf()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                
                var labels = await GetRequiredStreams(provider, "labels");
                var variablesJson = await GetRequiredParameter<string>(provider, "variables");
                var variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(variablesJson);
                var outputFileName = await GetRequiredParameter<string>(provider, "outputFileName");
                var appendToFile = await GetRequiredParameter<bool>(provider, "appendToFile");

                _labelService.PrintToPdf(labels, variables, outputFileName, appendToFile);

                return Ok("Printing labels");
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(message);
            }
        }
        
        [HttpPost]
        [Route("printLabelBatch")]
        public async Task<IHttpActionResult> PrintLabelBatch()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                
                var labels = await GetRequiredStreams(provider, "labels");
                var variablesJson = await GetRequiredParameter<string>(provider, "variables");
                var variables = JsonConvert.DeserializeObject<Dictionary<string, string>>(variablesJson);
                var printerSettingsJson = await GetRequiredParameter<string>(provider, "printerSettings");
                var printerSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(printerSettingsJson);
                var printerName = await GetOptionalParameter(provider, "printerName");

                _labelService.PrintLabelBatch(labels, variables, printerSettings, printerName);

                return Ok("Printing labels");
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(message);
            }
        }

        [HttpGet]
        [Route("printers")]
        public IHttpActionResult GetPrinters()
        {
            return Ok(_labelService.GetPrinters());
        }
        
        [HttpPost]
        [Route("getLabelDetails")]
        public async Task<IHttpActionResult> GetLabelDetails()
        {
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var label = await GetRequiredStream(provider, "label");
                var labelDetails = _labelService.GetLabelDetails(label);

                return Ok(labelDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
        
        private async Task<List<Stream>> GetRequiredStreams(MultipartMemoryStreamProvider provider, string paramName)
        {
            var contents = provider.Contents
                .Where(c => c.Headers.ContentDisposition?.Name?.Trim('"') == paramName)
                .ToList();
            if (contents.Count == 0) throw new ValidationException($"{paramName} must be present");

            var streams = new List<Stream>();
            foreach (var content in contents)
            {
                streams.Add(await content.ReadAsStreamAsync());
            }
            return streams;
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