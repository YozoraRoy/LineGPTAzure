using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using OpenAI_API;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net.Http.Headers;
using LineGPTAzureFunctions.MessageClass;
using OpenAI_API.Moderation;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using LineGPTAzureFunctions.Line;
using LineGPTAzureFunctions.ChatGPT;
using LineGPTAzureFunctions.DB;
using System.Reflection.Metadata;
using System.Configuration;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.Azure.Cosmos;

namespace LineGPTAzureFunctions
{
    public static class LineReply
    {
        static LineProcess lineProcess = new LineProcess();
        static string lineUserId = string.Empty;
        static string lineUserName = string.Empty;
        static string xLineSignature = string.Empty;

        [FunctionName("LineReply")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest HttpRequest,
            ILogger _logger)
        {

            _logger.LogInformation($"start");
            HttpRequest.Headers.TryGetValue("X-Line-Signature", out var EmxLineSignature);
            string requestBody = await new StreamReader(HttpRequest.Body).ReadToEndAsync();

            if (!EmxLineSignature.Any())
            {
                _logger.LogError($"X-Line-Signature is null HttpRequest:{HttpRequest} ,Body : {requestBody}");
                return new BadRequestResult();
            }

            xLineSignature = EmxLineSignature.FirstOrDefault();

            var jsonFromLine = System.Text.Json.JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);
            string lineType = jsonFromLine.events[0].type;
            lineUserId = jsonFromLine.events[0].source.userId;

            string lineMessagetype = jsonFromLine.events[0].message.type;
            string lineReplayToken = jsonFromLine.events[0].replyToken;
            var lineUserData = await lineProcess.GetUserProfile(lineUserId, string.Empty);
            lineUserName = lineUserData.displayName;
            _logger.LogInformation($"lineUserName: {lineUserName} / lineUserId: {lineUserId}");
            try
            {
                if (lineProcess.IsSingature(xLineSignature, requestBody))
                {
                    _logger.LogInformation($"lineProcess: OK");

                    if (lineMessagetype == "audio")
                    {
                        string lineMessageId = jsonFromLine.events[0].message.id;
                        LineAudio lineAudio = new LineAudio(_logger);
                        string TextResult = await lineAudio.ProcessWithAzureForSteam(lineMessageId);
                        // Check conversation form azure cosmosdb replaccr and store
                        await ProcChatGTPandLineReply(_logger, lineUserId, lineReplayToken, lineUserData, TextResult);

                        return new OkResult();
                    }
                    else if (lineMessagetype == "sticker"
                        || lineMessagetype == "image"
                        || lineMessagetype == "video"
                        || lineMessagetype == "location"
                        || lineMessagetype == "uri"
                        )
                    {
                        await lineProcess.ReplyAsync(lineReplayToken,
                            @"Sorry we are not support sticker / image / video / uri 。
                              申し訳ありませんが、ステッカー、画像、動画、URIのサポートは行っておりません。", lineUserName, lineUserId);                        
                        return new OkResult();
                    }
                    else
                    {
                        string lineMessage = jsonFromLine.events[0].message.text;
                        _logger.LogInformation($"Message: {lineMessage}");

                        if (lineMessage == "自訂角色")
                        {
                            string s1 = ConfigurationManager.AppSettings["_master"];
                            string s2 = Environment.GetEnvironmentVariable("_master");
                            string s3 = ConfigurationManager.AppSettings["default"];
                            string s4 = Environment.GetEnvironmentVariable("default");

                            await lineProcess.ReplyAsync(lineReplayToken, $"{s1}/{s2}/{s3}/{s4}", lineUserName, lineUserId);

                            return new OkResult();
                        }

                        // Check conversation form azure cosmosdb replaccr and store
                        await ProcChatGTPandLineReply(_logger, lineUserId, lineReplayToken, lineUserData, lineMessage);

                        return new OkResult();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                string msg = $"From [HttpRequestException !!][User:{lineUserName} || UserID {lineUserId}] process exception error {ex.Message} || {requestBody}";
                _logger.LogError($"User:{lineUserName} || UserID:{lineUserId} || {msg}");
                string msgforLine = "Sorry, we are currently experiencing a network or server error and are working on it..";
                if (msg.Contains("16385 "))
                {
                    msgforLine = @"This conversation has exceeded the maximum number of words provided by OPAI, and will soon enter the automatic self erase memory process. ";
                }

                await lineProcess.ReplyAsync(lineReplayToken, msgforLine, lineUserName, lineUserId);
                _ = lineProcess.SendNotify(msg);

                // Clear All data by user id.
                await ClearCosmosDB(_logger, lineUserId);

                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                string msg = $"From [OutSide Exception!!] process exception error {ex.Message} || {requestBody}";
                _logger.LogError($"User:{lineUserName} || UserID {lineUserId} || {msg}");
                await lineProcess.ReplyAsync(lineReplayToken, "Currently under repair, please try again later", lineUserName, lineUserId);
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }

            _logger.LogError($"User:{lineUserName} From Line process exception error...");
            await lineProcess.ReplyAsync(lineReplayToken, @"Currently under repair, please try again later。", lineUserName, lineUserId);
            return new BadRequestResult();
        }


        [FunctionName("LineReplyWithAssistant")]
        public static async Task<IActionResult> LineReplyWithAssistant(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest HttpRequest, ILogger _logger)
        {

            _logger.LogInformation($"start");
            HttpRequest.Headers.TryGetValue("X-Line-Signature", out var EmxLineSignature);
            string requestBody = await new StreamReader(HttpRequest.Body).ReadToEndAsync();

            if (!EmxLineSignature.Any())
            {
                _logger.LogError($"X-Line-Signature is null HttpRequest:{HttpRequest} ,Body : {requestBody}");
                return new BadRequestResult();
            }

            xLineSignature = EmxLineSignature.FirstOrDefault();
            var jsonFromLine = System.Text.Json.JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);
            if (jsonFromLine != null)
            {
                lineUserId = jsonFromLine.events[0].source.userId;
            }
            else
            {
                _logger.LogError($"requestBody is null HttpRequest:{HttpRequest} ,Body : {requestBody}");
                return new BadRequestResult();
            }

            string lineMessagetype = jsonFromLine.events[0].message.type;
            string lineReplayToken = jsonFromLine.events[0].replyToken;
            string linechannelAccessToken = "jbwg5RbX/5A47Gg/xGgvMVE0WMFNjlpYzDrc2fyAGO07qikKownm3Wu4u7mrTbu15VgoqZgq/RfGo2RM0WlgHGpw/gSSa/BWNyGYx8tJaNioffVXTiGUBnjbsSNzijJEkAy9GA3w9XQKZCiCb7SLxwdB04t89/1O/w1cDnyilFU=";
            string linechannelSecret = "1045973fc88d32d25dd2eb22586ddbed";
            LineProcess lineProcess = new LineProcess();
            var lineUserData = await lineProcess.GetUserProfile(lineUserId, linechannelAccessToken);
            lineUserName = lineUserData.displayName;

            _logger.LogInformation($"lineProcess.IsSingature(xLineSignature, requestBody)");

            try
            {

                if (lineProcess.IsSingature(xLineSignature, requestBody, linechannelSecret))
                {
                    _logger.LogInformation($"if (lineProcess.IsSingature(xLineSignature.... Success");

                    if (lineMessagetype == "audio")
                    {
                        string lineMessageId = jsonFromLine.events[0].message.id;

                        return new OkResult();
                    }
                    else if (lineMessagetype == "sticker"
                        || lineMessagetype == "image"
                        || lineMessagetype == "video"
                        || lineMessagetype == "location"
                        || lineMessagetype == "uri"
                        )
                    {
                        await lineProcess.ReplyAsync(lineReplayToken,
                            @"Sorry we are not support sticker / image / video / uri 。", lineUserName, lineUserId);
                        return new OkResult();
                    }
                    else
                    {
                        string lineMessage = jsonFromLine.events[0].message.text;

                        _logger.LogInformation($"Message: {lineMessage}");

                        // 寫入非Azure同步資料庫
                        // ChatDataAccess chatDataAccess = new ChatDataAccess();

                        string chatUserId = lineUserId;

                        // 新增使用者
                        // saveToAzureMSDB(chatDataAccess, chatUserId);

                        _logger.LogInformation($"start");
                        Assistant assistant = new Assistant(_logger);
                        var assistantResponse = await assistant.JPTeacherHelper(lineMessage).ConfigureAwait(false);

                        string threadId = string.Empty;
                        string assistantMsg = string.Empty;
                        foreach (var item in assistantResponse.Data)
                        {
                            threadId = item.ThreadId;
                            if (item.Role == "assistant")
                            {
                                assistantMsg = item.ContentList[0].Text.Value;

                                if (string.IsNullOrEmpty(assistantMsg))
                                {
                                    assistantMsg = "剛剛網路狀況有點不好，請再問一次剛剛的問題。";
                                    Console.WriteLine($"AI: {assistantMsg}");
                                }
                                else
                                {
                                    Console.WriteLine($"AI: {assistantMsg}");
                                }
                            }
                        }

                        await lineProcess.ReplyAsync(lineReplayToken, $"{assistantMsg}", lineUserName, lineUserId).ConfigureAwait(false);
                        _logger.LogInformation($"StoreChatMsgToContainerAsync:start");


                        // 新增使用者的問題
                       // SaveMessageToAzureMSDB(chatDataAccess, chatUserId, lineMessage, ChatRole.User.ToString(), "002", 3, threadId);

                       // SaveMessageToAzureMSDB(chatDataAccess, chatUserId, assistantMsg, ChatRole.Assistant.ToString(), "002", 3, threadId);

                        _logger.LogInformation($"ResultMessage:{assistantMsg}");
                        return new OkResult();
                    }
                }
                else
                {
                    string msg = $"From [xLineSignature  Error || {requestBody}";
                    _logger.LogError($"User:{lineUserName} || UserID {lineUserId} || {msg}");
                }
            }
            catch (HttpRequestException ex)
            {
                string msg = $"From [HttpRequestException !!][User:{lineUserName} || UserID {lineUserId}] process exception error {ex.Message} || {requestBody}";
                //  _logger.LogError($"User:{lineUserName} || UserID:{lineUserId} || {msg}");
                string msgforLine = "Sorry, we are currently experiencing a network or server error and are working on it..";
                //if (msg.Contains("16385 "))
                //{
                //    msgforLine = @"This conversation has exceeded the maximum number of words provided by OPAI, and will soon enter the automatic self erase memory process. ";
                //}

                // 暫時不使用
                //await lineProcess.ReplyAsync(lineReplayToken, msgforLine);
                //_ = lineProcess.SendNotify(msg);

                // Clear All data by user id.
                // await ClearCosmosDB(log, lineUserId);

                return new BadRequestResult();
            }
            catch (TaskCanceledException cx)
            {
                string msg = $"From [TaskCanceledException  r {cx.Message} || {requestBody}";
                _logger.LogError($"User:{lineUserName} || UserID {lineUserId} || {msg}");
                await lineProcess.ReplyAsync(lineReplayToken, "The internet connection was a bit unstable just now, please ask the previous question again, or try again later.", lineUserName, lineUserId);
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }

            catch (Exception ex)
            {
                string msg = $"From [OutSide Exception!!] process exception error {ex.Message} || {requestBody}";
                _logger.LogError($"User:{lineUserName} || UserID {lineUserId} || {msg}");
                await lineProcess.ReplyAsync(lineReplayToken, "We’ve had a little mishap now, please try again later.", lineUserName, lineUserId);
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }

            _logger.LogError($"User:{lineUserName} From Line process exception error...");
            await lineProcess.ReplyAsync(lineReplayToken, @"We’ve had a little mishap now, please try again later.", lineUserName, lineUserId);
            return new BadRequestResult();
        }


        private static async Task ProcChatGTPandLineReply(ILogger log, string lineUserId, string lineReplayToken, UserProfile lineUserData, string lineMessage)
        {
            CosmosProcess cosmosProcess = new CosmosProcess(log);
            List<ChatMessage> chatMessageList = await cosmosProcess.ChatGPTMessagePorcAsync(lineUserId, lineUserName, lineMessage);
            ChatMessage[] messages = chatMessageList.ToArray();
            ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
            ChatResult resultsOfchatGPTProcess = await chatGPTProcess.StartEndpointMode(log, messages);

            await lineProcess.ReplyAsync(lineReplayToken, resultsOfchatGPTProcess.Choices[0].Message.Content.Trim(), lineUserName, lineUserId);
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, resultsOfchatGPTProcess.Choices[0].Message.Content.Trim()));
            await cosmosProcess.FinalMessageDataProcess(chatMessageList, lineUserId, lineUserName);
            chatMessageList.Clear();
        }

        private static async Task ClearCosmosDB(ILogger log, string lineUserId)
        {
            CosmosProcess cosmosProcess = new CosmosProcess(log);
            await cosmosProcess.DeleteItemAsyncWithInstance(lineUserId);
        }
    }
}
