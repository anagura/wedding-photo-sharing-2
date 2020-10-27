using functions.Const;
using functions.Model;
using functions.Service;
using functions.TextTemplates;
using functions.Utility;
using LineMessaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static functions.Configration.EnvironmentVariables;
using static functions.Const.FunctionsConst;

namespace WeddingPhotoSharing
{
    public class LineReceiver
    {
        static LineMessagingClient lineMessagingClient;

        private readonly ComputerVisionService _computerVisionService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="computerVisionService"></param>
        public LineReceiver(ComputerVisionService computeVisionService)
        {
            _computerVisionService = computeVisionService;
        }

        [FunctionName("LineReceiver")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            try
            {
                var webhookRequest = new LineWebhookRequest(LineChannelSecret, req);
                var valid = await webhookRequest.IsValid();
                if (!valid)
                {
                    log.LogError("request is invalid.");
                    return null;
                }

                lineMessagingClient = new LineMessagingClient(LineAccessToken);

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
                        string suffix = string.Empty;

                        LineResult result = new LineResult()
                        {
                            Id = eventMessage.Message.Id,
                            Name = name,
                            MessageType = (int)eventMessage.Message.Type,
                        };
                        if (eventMessage.Message.Type == MessageType.Text)
                        {
                            string textMessage = eventMessage.Message.Text;
                            var maxLength = textMessage.Length > MessageMaxLength ? MessageMaxLength : textMessage.Length;
                            if (textMessage.Length > MessageMaxLength)
                            {
                                textMessage = textMessage.Substring(0, MessageMaxLength) + "...";
                                suffix = $"{Environment.NewLine}メッセージが長いため、途中までしか表示されません。{MessageMaxLength.ToString()}文字以内で入力をお願いします。";
                            }

                            // テンプレートよりランダム抽出
                            var template = TextImageTemplateAccessor.PickRamdom(maxLength);

                            // テンプレートよりバイト配列生成
                            byte[] image = ImageGenerator.Generate(template, result.Name, textMessage);

                            // ストレージテーブルに格納
                            await StorageUtil.UploadImage(
                                image, fileName);

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

                            // 画像チェック
                            var analyzeResult = await _computerVisionService.AnalyzeImageAsync(lineResult.Result);
                            var (IsAbnormal, AbnormalRate) = _computerVisionService.CheckImageAnalysis(analyzeResult);
                            if (IsAbnormal)
                            {
                                // アダルト用ストレージにアップロード
                                await StorageUtil.UploadImage(
                                    lineResult.Result, fileName, BlobContainerType.Adult);

                                var rate = AbnormalRate.ToString();
                                await ReplyToLine(eventMessage.ReplyToken, $"ちょっと嫌な予感がするので、この写真は却下します。{Environment.NewLine}不快画像確率:{rate}%", log);
                                continue;
                            }

                            // 画像をストレージにアップロード
                            await StorageUtil.UploadImage(lineResult.Result, fileName);

                            // サムネイル化
                            var thumbnailFileName = string.Empty;
                            var thumbnailStream = await _computerVisionService.GenerateThumbnailStreamAsync(
                                lineResult.Result, 
                                analyzeResult.Metadata.Width / 4,
                                analyzeResult.Metadata.Height / 4,
                                true);
                            if (thumbnailStream != null)
                            {
                                thumbnailFileName = $"thumbnail_{fileName}";
                                await StorageUtil.UploadImage(StorageUtil.GetByteArrayFromStream(thumbnailStream), thumbnailFileName);

                                // tableにアップロード
                                var imageFullPath = StorageUtil.GetImageFullPath(fileName);
                                await UploadMessageToStorageTable(eventMessage.Message.Id,
                                    name, string.Empty, imageFullPath,
                                    StorageUtil.GetImageFullPath(thumbnailFileName), imageFullPath);
                            }
                        }
                        else
                        {
                            log.LogError("not supported message type:" + eventMessage.Message.Type);
                            await ReplyToLine(eventMessage.ReplyToken, "未対応のメッセージです。テキストか画像を投稿してください", log);
                            continue;
                        }

                        await ReplyToLine(eventMessage.ReplyToken, $"投稿を受け付けました。表示されるまで少々お待ちください。{suffix}", log);

                        lineResults.Add(result);
                    }
                }

                return new OkObjectResult(lineResults.Any() ? JsonSerializer.Serialize(lineResults) : string.Empty);

            }
            catch (Exception e)
            {
                log.LogError($"Exception occured. Exception: {e} StackTrace: {e.StackTrace}");
                throw;
            }
        }

        private static async Task ReplyToLine(string replyToken, string message, ILogger log)
        {
            try
            {
                await lineMessagingClient.ReplyMessage(replyToken, message);
            }
            catch (LineMessagingException lex)
            {
                log.LogError($"message:{lex.Message}, source:{lex.Source}, token:{replyToken}");
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
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
                ThunbnailImageUrl= thumbnailImageUrl,
                RawImageUrl = rawImageUrl
            };

            return await StorageUtil.UploadMessageAsync(tableMessage);
        }
    }
}