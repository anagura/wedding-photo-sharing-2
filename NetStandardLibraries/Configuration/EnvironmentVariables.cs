using System;

namespace NetStandardLibraries.Configration
{
    /// <summary>
    /// 環境変数取得用クラス
    /// </summary>
    public static class EnvironmentVariables
    {
        private static string lineChannelSecret;
        public static string LineChannelSecret
            => lineChannelSecret
            ??= GetEnvironmentVariable("ChannelSecret");

        private static string lineAccessToken;
        public static string LineAccessToken
            => lineAccessToken
            ??= GetEnvironmentVariable("LineAccessToken");

        private static string lineMediaContainerName;
        public static string LineMediaContainerName
            => lineMediaContainerName
            ??= GetEnvironmentVariable("LineMediaContainerName");

        private static string lineAdultMediaContainerName;
        public static string LineAdultMediaContainerName
            => lineAdultMediaContainerName
            ??= GetEnvironmentVariable("LineAdultMediaContainerName");

        private static string lineMessageTableName;
        public static string LineMessageTableName
            => lineMessageTableName
            ??= GetEnvironmentVariable("LineMessageTableName");

        private static string storageAccountName;
        public static string StorageAccountName
            => storageAccountName
            ??= GetEnvironmentVariable("StorageAccountName");

        private static string storageAccountKey;
        public static string StorageAccountKey
            => storageAccountKey
            ??= GetEnvironmentVariable("StorageAccountKey");

        private static string visionSubscriptionKey;
        public static string VisionSubscriptionKey
            => visionSubscriptionKey
            ??= GetEnvironmentVariable("VisionSubscriptionKey");

        private static string storageImageDomainName;
        public static string StorageImageDomainName
            => storageImageDomainName
            ??= GetEnvironmentVariable("StorageImageDomainName");

        private static string lineMediaContainerUrlTableName;
        public static string LineMediaContainerUrlTableName
            => lineMediaContainerUrlTableName
            ??= GetEnvironmentVariable("LineMediaContainerUrlTableName");

        private static string imageConversionApiKey;
        public static string ImageConversionApiKey
            => imageConversionApiKey
            ??= GetEnvironmentVariable("ImageConversionApiKey");

        private static string imageConversionUri;
        public static string ImageConversionUri
            => imageConversionUri
            ??= GetEnvironmentVariable("ImageConversionUri");

        /// <summary>
        /// 指定keyで環境変数より値を取得
        /// </summary>
        /// <param name="key"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string GetEnvironmentVariable(string key,
            EnvironmentVariableTarget target = EnvironmentVariableTarget.Process)
            => Environment.GetEnvironmentVariable(key, target);
    }
}
