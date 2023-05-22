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
            log.LogInformation($"Message: {jsonFromLine.events[0].message.text}");

            var lineUserData = await lineProcess.GetUserProfile(jsonFromLine.events[0].source.userId);
            log.LogInformation($"UserData: {lineUserData.displayName}");

            try
            {

                if (lineProcess.IsSingature(xLineSignature, requestBody))
                {
                    log.LogInformation($"lineProcess: OK");

                    if (jsonFromLine.events[0].type == "message")
                    {
                        // Gheck conversion form azure cosmosdb replaccr and store
                        CosmosProcess cosmosProcess = new CosmosProcess(log);
                        var chatMessageList = await cosmosProcess.ChatGPTMessagePorcAsync(jsonFromLine.events[0].source.userId, lineUserData.displayName, jsonFromLine.events[0].message.text);
                        ChatMessage[] messages = chatMessageList.ToArray();
                        chatMessageList.Clear();

                        ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
                        ChatResult results = await chatGPTProcess.StartEndpointMode(log, messages);
                        if (string.IsNullOrEmpty(results.ToString()))
                        {
                            log.LogError($"From [OPenAI Error!!] process exception error...");
                            await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken, "From [Other!!]  some process exception error...");
                            return new BadRequestResult();
                        }

                        await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken, results.Choices[0].Message.Content.Trim());
                        return new OkResult();
                    }
                    else if (jsonFromLine.events[0].type == "sticker"
                        || jsonFromLine.events[0].type == "image"
                        || jsonFromLine.events[0].type == "video"
                        || jsonFromLine.events[0].type == "audio"
                        //|| json.events[0].type == "location"
                        //|| json.events[0].type == "uri"
                        )
                    {
                        await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken,
                            "Sorry we are not support sticker / image / video / audio  ");
                    }
                }

            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
                //await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken, "From some process exception error...");
                return new BadRequestResult();
            }

            log.LogError($"From Line process exception error...");
            await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken, "From some process exception error...");
            return new BadRequestResult();

            //log.LogError($"From [Other!!] process exception error...");
            //await lineProcess.ReplyAsync(jsonFromLine.events[0].replyToken, "From [Other!!]  some process exception error...");
            //return new BadRequestResult();

        }
    }
}
