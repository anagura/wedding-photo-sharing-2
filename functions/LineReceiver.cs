﻿using functions.Extension;
using functions.Model;
using functions.TextTemplates;
using functions.Utility;
using LineMessaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WeddingPhotoSharing
{
    public static class LineReceiver
    {
        static readonly string ChannelSecret = Environment.GetEnvironmentVariable("ChannelSecret", EnvironmentVariableTarget.Process);
        static readonly string LineAccessToken = Environment.GetEnvironmentVariable("LineAccessToken", EnvironmentVariableTarget.Process);

        static readonly string StorageAccountName = Environment.GetEnvironmentVariable("StorageAccountName", EnvironmentVariableTarget.Process);
        static readonly string StorageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey", EnvironmentVariableTarget.Process);
        static readonly string LineMediaContainerName = Environment.GetEnvironmentVariable("LineMediaContainerName", EnvironmentVariableTarget.Process);
        static readonly string LineAdultMediaContainerName = Environment.GetEnvironmentVariable("LineAdultMediaContainerName", EnvironmentVariableTarget.Process);
        static readonly string LineMessageTableName = Environment.GetEnvironmentVariable("LineMessageTableName", EnvironmentVariableTarget.Process);

        static readonly string VisionSubscriptionKey = Environment.GetEnvironmentVariable("VisionSubscriptionKey", EnvironmentVariableTarget.Process);
        static readonly string VisionUrl = "https://japaneast.api.cognitive.microsoft.com/vision/v2.0/analyze";

        const int MessageLength = 40;

        static LineMessagingClient lineMessagingClient;

        // Azure Storage
        static StorageCredentials storageCredentials;
        static CloudStorageAccount storageAccount;

        // 画像格納のBlob
        static CloudBlobClient blobClient;
        static CloudBlobContainer container;
        static CloudBlobContainer adultContainer;

        // メッセージ格納のTable
        static CloudTableClient tableClient;
        static CloudTable table;

        [FunctionName("LineReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                var webhookRequest = new LineWebhookRequest(ChannelSecret, req);
                var valid = await webhookRequest.IsValid();
                if (!valid)
                {
                    log.LogError("request is invalid.");
                    return null;
                }

                lineMessagingClient = new LineMessagingClient(LineAccessToken);

                storageCredentials = new StorageCredentials(StorageAccountName, StorageAccountKey);
                storageAccount = new CloudStorageAccount(storageCredentials, true);
                blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference(LineMediaContainerName);
                adultContainer = blobClient.GetContainerReference(LineAdultMediaContainerName);

                tableClient = storageAccount.CreateCloudTableClient();
                table = tableClient.GetTableReference(LineMessageTableName);
                await table.CreateIfNotExistsAsync();

                var message = await webhookRequest.GetContentJson();
                log.LogInformation(message);
                List<LineResult> lineResults = new List<LineResult>();

                LineWebhookContent content = await webhookRequest.GetContent();
                foreach (LineWebhookContent.Event eventMessage in content.Events)
                {
                    if (eventMessage.Type == WebhookRequestEventType.Message
                        && eventMessage.Source.Type == WebhookRequestSourceType.User)
                    {
                        string userId = eventMessage.Source.UserId;
                        var profile = await lineMessagingClient.GetProfile(userId);
                        var name = profile.DisplayName;
                        var ext = eventMessage.Message.Type == MessageType.Video ? ".mpg" : ".jpg";
                        var fileName = eventMessage.Message.Id.ToString() + ext;
                        string suffix = "";

                        LineResult result = new LineResult()
                        {
                            Id = eventMessage.Message.Id,
                            Name = name,
                            MessageType = (int)eventMessage.Message.Type,
                        };
                        if (eventMessage.Message.Type == MessageType.Text)
                        {
                            string textMessage = eventMessage.Message.Text;
                            var maxLength = textMessage.Length > MessageLength ? MessageLength : textMessage.Length;
                            if (textMessage.Length > MessageLength)
                            {
                                textMessage = textMessage.Substring(0, MessageLength) + "...";
                                suffix = string.Format("\nメッセージが長いため、途中までしか表示されません。{0}文字以内で入力をお願いします。", MessageLength);
                            }

                            // テンプレートよりランダム抽出
                            var template = TextImageTemplateAccessor.PickRamdom(maxLength);

                            // xamlよりバイト配列生成
                            byte[] image = ImageGenerator.Generate(template, result.Name, textMessage);

                            // ストレージテーブルに格納
                            await UploadImageToStorage(fileName, image);

                            result.Message = textMessage;
                        }
                        else if (eventMessage.Message.Type == MessageType.Image)
                        {
                            // LINEから画像を取得
                            var lineResult = lineMessagingClient.GetMessageContent(eventMessage.Message.Id.ToString());
                            if (lineResult.Result == null || !lineResult.IsCompleted || lineResult.IsFaulted)
                            {
                                throw new Exception("GetMessageContent is null");
                            }

                            // エロ画像チェック
                            string vision_result = await MakeAnalysisRequest(lineResult.Result, log);

                            var vision = JsonConvert.DeserializeObject<VisionAdultResult>(vision_result);
                            if (vision.Adult.isAdultContent)
                            {
                                // アダルト用ストレージにアップロード
                                await UploadImageToStorage(fileName, lineResult.Result, true);

                                vision_result += ", imageUrl:" + GetUrl(fileName, true);
                                await ReplyToLine(eventMessage.ReplyToken, string.Format("ちょっと嫌な予感がするので、この写真は却下します。\nアダルト画像確率:{0}%", Math.Round(vision.Adult.adultScore * 100, 0, MidpointRounding.AwayFromZero)), log);
                                continue;
                            }

                            // 画像をストレージにアップロード
                            await UploadImageToStorage(fileName, lineResult.Result);

                            result.ImageUrl = GetUrl(fileName);
                        }
                        else
                        {
                            log.LogError("not supported message type:" + eventMessage.Message.Type);
                            await ReplyToLine(eventMessage.ReplyToken, "未対応のメッセージです。テキストか画像を投稿してください", log);
                            continue;
                        }

                        await ReplyToLine(eventMessage.ReplyToken, "投稿を受け付けました。表示されるまで少々お待ちください。" + suffix, log);

                        lineResults.Add(result);
                    }
                }

                return new OkObjectResult(lineResults.Any() ? JsonConvert.SerializeObject(lineResults) : string.Empty);

            }
            catch (Exception e)
            {
                log.LogError($"Exception occured. Exception: { e } StackTrace: {e.StackTrace }");
                throw;
            }
        }

        private static async Task<string> MakeAnalysisRequest(byte[] byteData, ILogger log)
        {
            string contentString = string.Empty;

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", VisionSubscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "visualFeatures=Adult";

                // Assemble the URI for the REST API Call.
                string uri = VisionUrl + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                //				  byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Make the REST API call.
                    response = await client.PostAsync(uri, content);
                }

                // Get the JSON response.
                contentString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                log.LogError("\n" + e.Message);
            }

            return contentString;
        }

        private static async Task ReplyToLine(string replyToken, string message, ILogger log)
        {
            try
            {
                await lineMessagingClient.ReplyMessage(replyToken, message);
            }
            catch (LineMessagingException lex)
            {
                log.LogError(string.Format("message:{0}, source:{1}, token:{2}", lex.Message, lex.Source, replyToken));
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        private static async Task UploadMessageToStorageTable(long id, string name, string message)
        {
            // テーブルストレージに格納
            LineMessageEntity tableMessage = new LineMessageEntity(name, id.ToString());
            tableMessage.Id = id;
            tableMessage.Name = name;
            tableMessage.Message = message;

            TableOperation insertOperation = TableOperation.Insert(tableMessage);
            await table.ExecuteAsync(insertOperation);
        }

        private static async Task UploadImageToStorage(string fileName, byte[] image, bool isAdult = false)
        {
            CloudBlockBlob blockBlob = isAdult ?
                adultContainer.GetBlockBlobReference(fileName) :
                container.GetBlockBlobReference(fileName);

            blockBlob.Properties.ContentType = "image/jpeg";

            await blockBlob.UploadFromByteArrayAsync(image, 0, image.Length);
        }

        public class VisionAdultResult
        {
            [JsonProperty("adult")]
            public VisionAdultResultDetail Adult { get; set; }
        }

        public class VisionAdultResultDetail
        {
            [JsonProperty("isAdultContent")]
            public bool isAdultContent { get; set; }

            [JsonProperty("adultScore")]
            public double adultScore { get; set; }

            [JsonProperty("isRacyContent")]
            public bool isRacyContent { get; set; }

            [JsonProperty("racyScore")]
            public double racyScore { get; set; }
        }

        private static string GetUrl(string fileName, bool isAdult = false)
        {
            return string.Format("https://{0}.blob.core.windows.net/{1}/{2}", StorageAccountName, isAdult ? LineAdultMediaContainerName : LineMediaContainerName, fileName);
        }

        public class LineResult
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("messageType")]
            public int MessageType { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }
        }
    }

}