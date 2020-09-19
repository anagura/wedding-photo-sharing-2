using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using functions.Configration;
using functions.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace functions.Utility
{
    public class StorageUtil
    {
        private static StorageUtil _instance;

        private static readonly string _accountName = AppSettings.Configuration["StorageAccountName"];
        private static readonly string _accountKey = AppSettings.Configuration["StorageAccountKey"];
        private static readonly string _imageContainer = AppSettings.Configuration["LineMediaContainerName"];
        private static readonly string _messageTableContainer = AppSettings.Configuration["LineMessageTableName"];
        private readonly CloudBlobContainer _container;
        private readonly CloudTable _messageContainer;

        private static readonly string _domain = AppSettings.Configuration["StorageImageDomainName"];

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(_accountName, _accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(_imageContainer);


            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            _messageContainer = tableClient.GetTableReference(_messageTableContainer);
        }

        public static string GetFullPath(string fileName)
        {
            return string.Format("https://{0}/{1}/{2}", _domain, _imageContainer, fileName);
        }

        private async Task<bool> UploadImageToStorage(Stream stream, string fileName)
        {
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = "image/jpeg";

            await blockBlob.UploadFromStreamAsync(stream);

            return true;
        }

        public static async Task<bool> UploadImage(Stream stream, string fileName)
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }

            await _instance.UploadImageToStorage(stream, fileName);

            return true;
        }
        public static async Task<LineMessageEntity> FetchMassage(string partitionKey, string rowKey)
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }

            return await _instance.FetchMassageFromTable(partitionKey, rowKey);
        }

        public async Task<LineMessageEntity> FetchMassageFromTable(string partitionKey, string rowKey)
        {
            LineMessageEntity result = null;

            try
            {
                var retrieveOperation = TableOperation.Retrieve<LineMessageEntity>(partitionKey, rowKey);
                TableResult retrievedResult = await _messageContainer.ExecuteAsync(retrieveOperation);

                result = retrievedResult.Result as LineMessageEntity;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }


    }
}
