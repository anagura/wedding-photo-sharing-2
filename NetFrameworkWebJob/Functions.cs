using NetFrameworkWebJob.Model;
using NetFrameworkWebJob.TextTemplates;
using NetFrameworkWebJob.Utility;
using ImageGeneration;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using NetStandardLibraries.Model;
using System;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetFrameworkWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static async Task ProcessQueueMessage([QueueTrigger("messages")] string message, TextWriter log)
        {
            var entity = JsonSerializer.Deserialize<MessageEntity>(message);

            // テンプレートよりランダム抽出
            var template = TextImageTemplateAccessor.PickRamdom(entity.Message.Length);

            // テンプレートよりバイト配列生成
            var inputXaml = template.Parse(entity.Name, entity.Message);
            dynamic viewModel = new ExpandoObject();
            viewModel.Name = entity.Name;
            viewModel.Text = DateTime.Now.Ticks.ToString();
            var image = ImageGenerator.GenerateImage(inputXaml, viewModel, Guid.NewGuid().ToString());

            var imageFullPath = StorageUtil.GetImageFullPath($"{entity.Id}.png");

            // 画像をストレージにアップロード
            await StorageUtil.UploadImage(image, $"{entity.Id}.png");

            await UploadMessageToStorageTable(entity.Id, entity.Name, entity.Message, imageFullPath, imageFullPath, imageFullPath);
        }
        private static async ValueTask<TableResult> UploadMessageToStorageTable(
            long id, string name, string message, string imageUrl, string thumbnailImageUrl, string rawImageUrl)
        {
            // テーブルストレージに格納
            var tableMessage = new LineMessageEntity(name, id.ToString())
            {
                Id = id,
                Name = name,
                Message = message,
                ImageUrl = imageUrl,
                ThunbnailImageUrl = thumbnailImageUrl,
                RawImageUrl = rawImageUrl
            };

            return await StorageUtil.UploadMessageAsync(tableMessage);
        }
    }
}
