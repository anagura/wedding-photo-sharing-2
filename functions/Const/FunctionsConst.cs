using System;

namespace functions.Const
{
    /// <summary>
    /// 定数
    /// </summary>
    public class FunctionsConst
    {
        public const string VisionUrl = "https://japaneast.api.cognitive.microsoft.com/vision/v2.0/analyze";

        public const int MessageLength = 40;

        public const int HttpClientRetryCountOnError = 3;

        public const int HttpClientHandledEventsAllowedBeforeBreaking = 5;

        public static readonly TimeSpan HttpClientDurationOnBreak = TimeSpan.FromSeconds(30);
    }
}
