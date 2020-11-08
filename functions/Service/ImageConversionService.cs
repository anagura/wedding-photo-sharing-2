using Microsoft.Extensions.Logging;
using NetStandardLibraries.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static NetStandardLibraries.Configration.EnvironmentVariables;

namespace functions.Service
{
    /// <summary>
    /// image変換サービス
    /// </summary>
    public class ImageConversionService
    {
        private readonly ILogger<ImageConversionService> _logger;

        private readonly HttpClient _httpClient;

        private static readonly string ImageConversionUrl =
            $"{ImageConversionUri}/api/ImageConversion";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        public ImageConversionService(
            ILogger<ImageConversionService> logger,
            IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Xamlからバイト配列に変換
        /// </summary>
        /// <param name="inputXaml"></param>
        /// <returns></returns>
        public async ValueTask<(HttpStatusCode StatusCode, byte[] ImageData)>
            ConvertFromXaml(string inputXaml)
        {
            var request = new ImageConversionRequest
            {
                ApiKey = ImageConversionApiKey,
                XamlData = inputXaml,
            };

            try
            {
                var jsonString = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(ImageConversionUrl, content);

                var statusCode = response.StatusCode;
                byte[] image = Enumerable.Empty<byte>().ToArray();

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var imageConversionResponse = JsonSerializer
                        .Deserialize<ImageConversionResponse>(responseString);
                    image = imageConversionResponse.ImageData;
                }

                return (statusCode, image);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occured during to post image conversion.");
                throw;
            }
        }
    }
}
