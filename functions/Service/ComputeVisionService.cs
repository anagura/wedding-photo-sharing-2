﻿using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
        private readonly ComputerVisionClient _computerVisionClient;
 
        /// <summary>
        /// 分析対象
        /// </summary>
        private static readonly List<VisualFeatureTypes> _visualFeatureList =
            new List<VisualFeatureTypes>
            {
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Adult,
            };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        public ComputeVisionService(
            ILogger<ComputeVisionService> logger,
            IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _computerVisionClient = new ComputerVisionClient(
            new ApiKeyServiceClientCredentials(VisionSubscriptionKey),
            httpClientFactory.CreateClient(), false)
            {
                Endpoint = ComputeVisionEndpoint,
            };
        }


        /// <summary>
        /// 画像分析
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async ValueTask<ImageAnalysis> AnalyzeImageAsync(byte[] buffer)
        {
            using var stream = new MemoryStream(buffer);
            return await _computerVisionClient.AnalyzeImageInStreamAsync(
                stream, _visualFeatureList);
        }

        /// <summary>
        /// サムネイル生成
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="smartCropping"></param>
        /// <returns></returns>
        public async ValueTask<Stream> GenerateThumbnailStreamAsync(
            byte[] buffer, int width, int height, bool smartCropping = false)
        {
            var standardHeight = ThumbnailImageStandardHeight;
            if (height > standardHeight)
            {
                var ratio = standardHeight / (float)height;
                var calcWidth = (int)(width * ratio);
                using var stream = new MemoryStream(buffer);
                return await _computerVisionClient.GenerateThumbnailInStreamAsync(
                    calcWidth, height, stream, smartCropping);
            }
            return null;
        }
    }
}