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
        [FunctionName("LineReply")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest HttpRequest,
            ILogger log)
        {

            HttpRequest.Headers.TryGetValue("X-Line-Signature", out var xLineSignature);
            string requestBody = await new StreamReader(HttpRequest.Body).ReadToEndAsync();
            log.LogInformation($"X-Line-Signature : {xLineSignature}");
            log.LogInformation($"Body : {requestBody}");

            var jsonFromLine = System.Text.Json.JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);
            string lineType = jsonFromLine.events[0].type;
            lineUserId = jsonFromLine.events[0].source.userId;

            string lineMessagetype = jsonFromLine.events[0].message.type;
            string lineReplayToken = jsonFromLine.events[0].replyToken;
            var lineUserData = await lineProcess.GetUserProfile(lineUserId);
            lineUserName = lineUserData.displayName;
            log.LogInformation($"lineUserName: {lineUserName} / lineUserId: {lineUserId}");
            try
            {
                if (lineProcess.IsSingature(xLineSignature, requestBody))
                {
                    log.LogInformation($"lineProcess: OK");

                    if (lineMessagetype == "audio")
                    {
                        string lineMessageId = jsonFromLine.events[0].message.id;
                        LineAudio lineAudio = new LineAudio(log);
                        string TextResult = await lineAudio.ProcessWithAzureForSteam(lineMessageId);
                        // Check conversation form azure cosmosdb replaccr and store
                        await ProcChatGTPandLineReply(log, lineUserId, lineReplayToken, lineUserData, TextResult);

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
                              申し訳ありませんが、ステッカー、画像、動画、URIのサポートは行っておりません。");                        
                        return new OkResult();
                    }
                    else
                    {
                        string lineMessage = jsonFromLine.events[0].message.text;
                        log.LogInformation($"Message: {lineMessage}");

                        if (lineMessage == "自訂角色")
                        {
                            string s1 = ConfigurationManager.AppSettings["_master"];
                            string s2 = Environment.GetEnvironmentVariable("_master");
                            string s3 = ConfigurationManager.AppSettings["default"];
                            string s4 = Environment.GetEnvironmentVariable("default");

                            await lineProcess.ReplyAsync(lineReplayToken, $"{s1}/{s2}/{s3}/{s4}");

                            return new OkResult();
                        }

                        // Check conversation form azure cosmosdb replaccr and store
                        await ProcChatGTPandLineReply(log, lineUserId, lineReplayToken, lineUserData, lineMessage);

                        return new OkResult();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                string msg = $"From [HttpRequestException !!][User:{lineUserName} || UserID {lineUserId}] process exception error {ex.Message} || {requestBody}";
                log.LogError($"User:{lineUserName} || UserID:{lineUserId} || {msg}");
                string msgforLine = "Sorry, we are currently experiencing a network or server error and are working on it..";
                if (msg.Contains("16385 "))
                {
                    msgforLine = @"This conversation has exceeded the maximum number of words provided by OPAI, and will soon enter the automatic self erase memory process. ";
                }

                await lineProcess.ReplyAsync(lineReplayToken, msgforLine);
                _ = lineProcess.SendNotify(msg);

                // Clear All data by user id.
                await ClearCosmosDB(log, lineUserId);

                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                string msg = $"From [OutSide Exception!!] process exception error {ex.Message} || {requestBody}";
                log.LogError($"User:{lineUserName} || UserID {lineUserId} || {msg}");
                await lineProcess.ReplyAsync(lineReplayToken, "Currently under repair, please try again later");
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }

            log.LogError($"User:{lineUserName} From Line process exception error...");
            await lineProcess.ReplyAsync(lineReplayToken, @"Currently under repair, please try again later。");
            return new BadRequestResult();
        }

        private static async Task ProcChatGTPandLineReply(ILogger log, string lineUserId, string lineReplayToken, UserProfile lineUserData, string lineMessage)
        {
            CosmosProcess cosmosProcess = new CosmosProcess(log);
            List<ChatMessage> chatMessageList = await cosmosProcess.ChatGPTMessagePorcAsync(lineUserId, lineUserName, lineMessage);
            ChatMessage[] messages = chatMessageList.ToArray();
            ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
            ChatResult resultsOfchatGPTProcess = await chatGPTProcess.StartEndpointMode(log, messages);

            await lineProcess.ReplyAsync(lineReplayToken, resultsOfchatGPTProcess.Choices[0].Message.Content.Trim());
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
