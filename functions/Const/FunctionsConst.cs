namespace functions.Const
{
    /// <summary>
    /// 定数
    /// </summary>
    public class FunctionsConst
    {
        public const string ComputeVisionEndpoint = "https://japaneast.api.cognitive.microsoft.com/";
        public static readonly string ComputeVisionAnalyzeUrl= $"{ComputeVisionEndpoint}vision/v2.0/analyze";
        private static readonly string ComputeVisionGenerateThumbnailUrl = $"{ComputeVisionEndpoint}vision/v3.0/generateThumbnail";

        public const int MessageLength = 40;

        public const int HttpClientRetryCountOnError = 3;
    }
}
