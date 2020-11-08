using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NetFrameworkFunctions.Utility;
using NetStandardLibraries.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static NetStandardLibraries.Configration.EnvironmentVariables;

namespace NetFrameworkFunctions
{
    public static class ImageConversion
    {
        [FunctionName("ImageConversion")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Image Conversion request start.");
            var response = new ImageConversionResponse();

            try
            {
                var request = await req.Content.ReadAsAsync<ImageConversionRequest>();
                if (ImageConversionApiKey != request.ApiKey)
                {
                    return req.CreateResponse(HttpStatusCode.Unauthorized, response);
                }
                var image = ImageConverter.ConvertFromXaml(request.XamlData);
                response.ImageData = image;
                log.Info("Image Conversion request finished.");
                return req.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                log.Error($"Error occured {nameof(e)}: {e.StackTrace}");
                return req.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }
    }
}
