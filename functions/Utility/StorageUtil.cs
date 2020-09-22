using functions.Const;
using functions.Model;
using functions.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static functions.Configration.EnvironmentVariables;

namespace functions.Utility
{
    public class StorageUtil
    {
        private static StorageUtil _instance;

        public readonly BlobContainerProvider _blobContainer;
        private readonly CloudBlobContainer _container;
        private readonly CloudTable _messageContainer;

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(StorageAccountName, StorageAccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = new BlobContainerProvider(blobClient);
            var tableClient = storageAccount.CreateCloudTableClient();
            _messageContainer = tableClient.GetTableReference(LineMessageTableName);
        }

        /// <summary>
        /// 画像フルパス取得
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetImageFullPath(string fileName)
        {
            return $"https://{StorageImageDomainName}/{LineMediaContainerName}/{fileName}";
        }

        private async ValueTask UploadImageToStorage(
            byte[] buffer,
            string fileName,
            BlobContainerType containerType)
        {
            var container = _blobContainer.GetBlobContainer(containerType);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = "image/jpeg";

            await blockBlob.UploadFromByteArrayAsync(buffer, 0, buffer.Length);
        }

        public static async ValueTask UploadImage(byte[] buffer,
            string fileName,
            BlobContainerType containerType = BlobContainerType.Normal)
        {
            _instance ??= new StorageUtil();
            await _instance.UploadImageToStorage(
                buffer, fileName, containerType);
        }
        public static async Task<List<LineMessageEntity>> FetchMassage()
        {
            _instance ??= new StorageUtil();
            return await _instance.FetchMassageFromTable();
        }

        public static async ValueTask<TableResult> UploadMessageAsync(ITableEntity entity)
        {
            _instance ??= new StorageUtil();

            return await _instance.UploadMessageToTableAsync(entity);
        }

        private async ValueTask<TableResult> UploadMessageToTableAsync(ITableEntity entity)
        {
            await _messageContainer.CreateIfNotExistsAsync();
            var insertOperation = TableOperation.InsertOrReplace(entity);
            return await _messageContainer.ExecuteAsync(insertOperation);
        }

        public async Task<List<LineMessageEntity>> FetchMassageFromTable()
        {
            List<LineMessageEntity> result = null;

            try
            {
                TableQuery<LineMessageEntity> query = new TableQuery<LineMessageEntity>();
                var list = await _messageContainer.ExecuteQuerySegmentedAsync(query, null);
                result = list.Results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }
    }
}
