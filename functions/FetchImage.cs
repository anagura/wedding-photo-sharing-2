using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using functions.Utility;

namespace functions
{
    public static class FetchImage
    {
        [FunctionName("FetchImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var result = await StorageUtil.GetLineMessageEntityList();
            return new OkObjectResult(JsonConvert.SerializeObject(result));
        }
    }
}
