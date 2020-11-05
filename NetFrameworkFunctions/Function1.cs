using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NetFrameworkFunctions.Utility;
using NetStandardLibraries.Model;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetFrameworkFunctions
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Image Conversion request start.");
            var response = new ImageConversionResponse();

            try
            {
                var request = await req.Content.ReadAsAsync<ImageConversionRequest>();
                var image = ImageConverter.GenerateFromXaml(request.XamlData);
                response.ImageData = image;
                log.Info("Image Conversion request finished.");
                return req.CreateResponse(HttpStatusCode.BadRequest, response);
            }
            catch (Exception e)
            {
                log.Error($"Error occured {nameof(e)}: {e.StackTrace}");
                return req.CreateResponse(HttpStatusCode.BadRequest, response);
            }
        }
    }
}
