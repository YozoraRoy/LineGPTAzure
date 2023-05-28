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
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.Azure.Cosmos;

namespace LineGPTAzureFunctions
{
    public static class LineReply
    {
        static LineProcess lineProcess = new LineProcess();

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
            string lineUserId = jsonFromLine.events[0].source.userId;

            string lineMessagetype = jsonFromLine.events[0].message.type;
            string lineReplayToken = jsonFromLine.events[0].replyToken;




            var lineUserData = await lineProcess.GetUserProfile(lineUserId);
            log.LogInformation($"UserData: {lineUserData.displayName}");
            try
            {
                if (lineProcess.IsSingature(xLineSignature, requestBody))
                {
                    log.LogInformation($"lineProcess: OK");

                    if (lineMessagetype == "audio")
                    {
                        if (lineUserData.displayName == "ローイ" || lineUserData.displayName == "Roy")
                        {
                            string lineMessageId = jsonFromLine.events[0].message.id;
                            LineAudio lineAudio = new LineAudio(log);
                            string TextResult = await lineAudio.ProcessWithAzureForSteam(lineMessageId);
                            // Check conversation form azure cosmosdb replaccr and store
                            await ProcChatGTPandLineReply(log, lineUserId, lineReplayToken, lineUserData, TextResult);
                        }
                        else
                        {
                            await lineProcess.ReplyAsync(lineReplayToken, "Sorry we are not support audio..");
                            _ = lineProcess.SendNotify($"{lineUserData.displayName}---{requestBody}");
                        }

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
                            "Sorry we are not support sticker / image / video / uri ..");
                        return new OkResult();
                    }
                    else
                    {
                        string lineMessage = jsonFromLine.events[0].message.text;
                        log.LogInformation($"Message: {lineMessage}");

                        if (lineMessage == "測試系統參數")
                        {
                            string s1 = string.Empty;
                            //string s1 = ConfigurationManager.AppSettings["_master"];
                            string s2 = Environment.GetEnvironmentVariable("_master");
                            string s3 = System.Configuration.ConfigurationManager.AppSettings["default"];
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
                string msg = $"From [HttpRequestException !!] process exception error {ex.Message} || {requestBody}";
                log.LogError(msg);
                string msgforLine = "Sorry, we are currently experiencing a network or server error and are working on it..";
                if (msg.Contains("4097"))
                {
                    msgforLine = "Sorry, you've provided more information than I can handle.";
                }

                await lineProcess.ReplyAsync(lineReplayToken, msgforLine);
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                string msg = $"From [OutSide Exception!!] process exception error {ex.Message} || {requestBody}";
                log.LogError(msg);
                await lineProcess.ReplyAsync(lineReplayToken, "Currently under repair, please try again later");
                _ = lineProcess.SendNotify(msg);
                return new BadRequestResult();
            }

            log.LogError($"From Line process exception error...");
            await lineProcess.ReplyAsync(lineReplayToken, "Currently under repair, please try again later");
            return new BadRequestResult();
        }

        private static async Task ProcChatGTPandLineReply(ILogger log, string lineUserId, string lineReplayToken, UserProfile lineUserData, string lineMessage)
        {
            CosmosProcess cosmosProcess = new CosmosProcess(log);
            List<ChatMessage> chatMessageList = await cosmosProcess.ChatGPTMessagePorcAsync(lineUserId, lineUserData.displayName, lineMessage);
            ChatMessage[] messages = chatMessageList.ToArray();
            ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
            ChatResult resultsOfchatGPTProcess = await chatGPTProcess.StartEndpointMode(log, messages);

            await lineProcess.ReplyAsync(lineReplayToken, resultsOfchatGPTProcess.Choices[0].Message.Content.Trim());
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, resultsOfchatGPTProcess.Choices[0].Message.Content.Trim()));
            await cosmosProcess.FinalMessageDataProcess(chatMessageList, lineUserId, lineUserData.displayName);
            chatMessageList.Clear();
        }
    }
}
