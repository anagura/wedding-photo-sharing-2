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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static functions.Configration.EnvironmentVariables;
using static functions.Const.FunctionsConst;

namespace WeddingPhotoSharing
{
    public class LineReceiver
    {
        static LineMessagingClient lineMessagingClient;

        private readonly ComputeVisionService _computeVisionService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="_computeVisionService"></param>
        public LineReceiver(ComputeVisionService computeVisionService)
        {
            _computeVisionService = computeVisionService;
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

                            // xamlよりバイト配列生成
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
                            var analyzeResult = await _computeVisionService.AnalyzeImageAsync(lineResult.Result);
                            var checkResult= _computeVisionService.CheckImageAnalysis(analyzeResult);
                            if (checkResult.IsAbnormal)
                            {
                                // アダルト用ストレージにアップロード
                                await StorageUtil.UploadImage(
                                    lineResult.Result, fileName, BlobContainerType.Adult);

                                var rate = checkResult.AbnormalRate.ToString();
                                await ReplyToLine(eventMessage.ReplyToken, $"ちょっと嫌な予感がするので、この写真は却下します。{Environment.NewLine}不快画像確率:{rate}%", log);
                                continue;
                            }

                            // 画像をストレージにアップロード
                            await StorageUtil.UploadImage(lineResult.Result, fileName);

                            // サムネイル化
                            var thumbnailStream = await _computeVisionService.GenerateThumbnailStreamAsync(
                                lineResult.Result, analyzeResult.Metadata.Width,
                                analyzeResult.Metadata.Height,
                                true);
                            if (thumbnailStream != null)
                            {
                                var thumbnailFileName = $"thumbnail_{fileName}";
                                await StorageUtil.UploadImage(lineResult.Result, thumbnailFileName);
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

                return new OkObjectResult(lineResults.Any() ? JsonConvert.SerializeObject(lineResults) : string.Empty);

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

        private static async ValueTask<TableResult> UploadMessageToStorageTable(long id, string name, string message)
        {
            // テーブルストレージに格納
            var tableMessage = new LineMessageEntity(name, id.ToString())
            {
                Id = id,
                Name = name,
                Message = message,
            };

            return await StorageUtil.UploadMessageToTableAsync(tableMessage);
        }

        private static string GetUrl(string fileName, bool isAdult = false)
        {
            var containerName = isAdult? LineAdultMediaContainerName : LineMediaContainerName;
            return $"https://{StorageAccountName}.blob.core.windows.net/{containerName}/{fileName}";
        }
    }
}