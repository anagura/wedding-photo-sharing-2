using System.IO;
using System.Threading.Tasks;
using functions.Configration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using static functions.Configration.EnvironmentVariables;

namespace functions.Utility
{
    public class StorageUtil
    {
        private static StorageUtil _instance;

        private readonly CloudBlobContainer _container;

        private static readonly string _domain = AppSettings.Configuration["StorageImageDomainName"];

        private StorageUtil()
        {
            StorageCredentials storageCredentials = new StorageCredentials(StorageAccountName, StorageAccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(LineMediaContainerName);
        }

        public static string GetFullPath(string fileName)
        {
            return string.Format("https://{0}/{1}/{2}", _domain, LineMediaContainerName, fileName);
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
            _instance ??= new StorageUtil();
            await _instance.UploadImageToStorage(stream, fileName);

            return true;
        }

    }
}
