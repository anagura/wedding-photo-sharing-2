using NetFrameworkWebJob.Const;
using NetFrameworkWebJob.Model;
using NetFrameworkWebJob.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static NetStandardLibraries.Configration.EnvironmentVariables;

namespace NetFrameworkWebJob.Utility
{
    public class StorageUtil
    {
        private static StorageUtil _instance;

        public readonly BlobContainerProvider _blobContainer;
        private readonly CloudTable _containerUrlTable;
        private readonly CloudQueue _containerUriQueue;

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(
                StorageAccountName, StorageAccountKey);

            CloudStorageAccount storageAccount = new CloudStorageAccount(
                storageCredentials, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _blobContainer = new BlobContainerProvider(blobClient);
            var tableClient = storageAccount.CreateCloudTableClient();
            _containerUrlTable = tableClient.GetTableReference(LineMediaContainerUrlTableName);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            _containerUriQueue = queueClient.GetQueueReference(LineTextContainerUrlQueueName);
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

        public static byte[] GetByteArrayFromStream(Stream sm)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                sm.CopyTo(ms);
                return ms.ToArray();
            }
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
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }
            await _instance.UploadImageToStorage(
                buffer, fileName, containerType);
        }
        public static async Task<List<LineMessageEntity>> FetchMassage()
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }
            return await _instance.FetchMassageFromTable();
        }

        public static async ValueTask<TableResult> UploadMessageAsync(ITableEntity entity)
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }
            return await _instance.UploadMessageToTableAsync(entity);
        }

        private async ValueTask<TableResult> UploadMessageToTableAsync(ITableEntity entity)
        {
            await _containerUrlTable.CreateIfNotExistsAsync();
            var insertOperation = TableOperation.InsertOrReplace(entity);
            return await _containerUrlTable.ExecuteAsync(insertOperation);
        }

        public async Task<List<LineMessageEntity>> FetchMassageFromTable()
        {
            List<LineMessageEntity> result = new List<LineMessageEntity>();

            try
            {
                TableQuery<LineMessageEntity> query = new TableQuery<LineMessageEntity>();
                var list = await _containerUrlTable.ExecuteQuerySegmentedAsync(query, null);
                result = list.Results;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static async Task InsertQueueAsync(string message)
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }
            await _instance.AddMessageToQueueAsync(message);
        }

        private async Task AddMessageToQueueAsync(string message)
        {
            await _containerUriQueue.CreateIfNotExistsAsync();
            await _containerUriQueue.AddMessageAsync(new CloudQueueMessage(message));
        }

    }
}
