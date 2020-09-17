using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static functions.Configration.EnvironmentVariables;
using static functions.Const.FunctionsConst;


namespace functions.Service
{
    /// <summary>
    /// Service for ComputeVision
    /// </summary>
    public class ComputeVisionService
    {
        private readonly ILogger<ComputeVisionService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ComputerVisionClient _computerVisionClient;

        private static readonly string GenerateThumbnailUrl = $"{ComputeVisionEndpoint}vision/v3.0/generateThumbnail";

        public ComputeVisionService(
            ILogger<ComputeVisionService> logger,
            IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            _computerVisionClient = new ComputerVisionClient(
            new ApiKeyServiceClientCredentials(VisionSubscriptionKey),
            httpClientFactory.CreateClient(), false)
            {
                Endpoint = ComputeVisionEndpoint,
            };
        }

        /// <summary>
        /// サムネイルのstream生成
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="smartCropping"></param>
        /// <returns></returns>
        public async ValueTask<Stream> GenerateThumbnailStreamAsync(
            byte[] buffer, int width, int height, bool smartCropping = false)
        {
            using var stream = new MemoryStream(buffer);
            return await _computerVisionClient.GenerateThumbnailInStreamAsync(
                width, height, stream, smartCropping);
        }

        /// <summary>
        /// 画像(stream)分析
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async ValueTask<ImageAnalysis> AnalyzeImageAsync(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            var visualFeatureList = new List<VisualFeatureTypes>();
            visualFeatureList.Add(VisualFeatureTypes.Adult);
            return await _computerVisionClient.AnalyzeImageInStreamAsync(
                stream, visualFeatureList);
        }
    }
}
