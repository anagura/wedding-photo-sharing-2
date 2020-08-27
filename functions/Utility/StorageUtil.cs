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
        private static readonly string LineMessageTableName = AppSettings.Configuration["LineMessageTableName"];
        private readonly CloudBlobContainer _container;

        // メッセージ格納のTable
        private static CloudTableClient tableClient;
        private static CloudTable table;
        private static readonly string _domain = AppSettings.Configuration["StorageImageDomainName"];

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(_accountName, _accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(_imageContainer);

            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(LineMessageTableName);

            table.CreateIfNotExistsAsync();
        }

        private static StorageUtil getInstance()
        {
            if (_instance == null)
            {
                _instance = new StorageUtil();
            }

            return _instance;
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
            await getInstance().UploadImageToStorage(stream, fileName);

            return true;
        }

        private async Task<List<LineMessageEntity>> getLineMessageEntityList()
        {
            TableQuery<LineMessageEntity> query = new TableQuery<LineMessageEntity>();

            List<LineMessageEntity> result = new List<LineMessageEntity>();
            try
            {
                foreach (LineMessageEntity entity in await table.ExecuteQuerySegmentedAsync(query, null))
                {
                    result.Add(entity);
                }

            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public static async Task<List<LineMessageEntity>> GetLineMessageEntityList()
        {
            return await getInstance().getLineMessageEntityList();
        }

    }
}
