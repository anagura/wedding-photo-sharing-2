using NetFrameworkWebJob.Const;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using static NetStandardLibraries.Configration.EnvironmentVariables;

namespace NetFrameworkWebJob.Storage
{
    /// <summary>
    /// BlobContainerプロバイダー
    /// </summary>
    public class BlobContainerProvider
    {
        private readonly CloudBlobClient _blobClient;

        /// <summary>
        /// BlobContainerのキー連想配列
        /// </summary>
        private static readonly Dictionary<BlobContainerType, string>
            _blobContainerKeys
            = new Dictionary<BlobContainerType, string>
            {
                {
                    BlobContainerType.Normal,
                    LineMediaContainerName
                },
                {
                    BlobContainerType.Adult,
                    LineAdultMediaContainerName
                },
            };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="blobClient"></param>
        public BlobContainerProvider(CloudBlobClient blobClient)
        {
            _blobClient = blobClient;
        }

        /// <summary>
        /// BlobContainer取得
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns></returns>
        public CloudBlobContainer GetBlobContainer(BlobContainerType containerType)
        {
            _blobContainerKeys.TryGetValue(containerType, out var key);
            return _blobClient.GetContainerReference(key);
        }
    }
}
