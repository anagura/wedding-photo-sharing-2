namespace functions.Const
{
    /// <summary>
    /// 定数
    /// </summary>
    public class FunctionsConst
    {
        public const string ComputeVisionEndpoint = "https://japaneast.api.cognitive.microsoft.com/";

        /// <summary>
        /// 画像のサムネイル基準height
        /// </summary>
        public const int ThumbnailImageStandardHeight = 300;

        /// <summary>
        /// メッセージ最大長
        /// </summary>
        public const int MessageMaxLength = 40;

        /// <summary>
        /// HTTPリトライ回数
        /// </summary>
        public const int HttpClientRetryCountOnError = 3;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MediaStorageType
    {
        /// <summary>
        /// 通常
        /// </summary>
        Normal,

        /// <summary>
        /// アダルト
        /// </summary>
        Adult,
    }
}
