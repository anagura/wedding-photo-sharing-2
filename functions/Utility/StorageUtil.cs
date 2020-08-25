using System.IO;
using System.Threading.Tasks;
using functions.Configration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace functions.Utility
{
    public class StorageUtil
    {
        private static StorageUtil _instance;

        private static readonly string _accountName = AppSettings.Configuration["AzureStorageAccountName"];
        private static readonly string _accountKey = AppSettings.Configuration["AzureStorageAccountKey"];
        private static readonly string _imageContainer = AppSettings.Configuration["AzureStorageImageContainerName"];
        private readonly CloudBlobContainer _container;

        private static readonly string _domain = AppSettings.Configuration["AzureStorageImageDomainName"];

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(_accountName, _accountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(_imageContainer);
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

    }
}
