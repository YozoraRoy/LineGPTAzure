using LineGPTAzureFunctions.Helper;
using LineGPTAzureFunctions.MessageClass;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.ChatGPT
{
    public class ChatGPTProcess
    {
        private ILogger log;
        private LineMessageReceiveJson json;
        private string  _apiKey = string.Empty;

        public ChatGPTProcess()
        {
        }

        public ChatGPTProcess(ILogger log, LineMessageReceiveJson json)
        {
            this.log = log;
            this.json = json;
        }

        public async Task<ChatResult> StartEndpointMode(ILogger log, ChatMessage[] messages)
        {
            ChatResult results = await ExecChatGpt(messages);
            return results;
        }

        public async Task<ChatResult> StartEndpointMode(ILogger log, string apiKey, ChatMessage[] messages)
        {
            _ = string.IsNullOrEmpty(apiKey) ? _apiKey = GetApikey() : _apiKey = apiKey;
            ChatResult results = await ExecChatGpt(messages);
            return results;
        }
      
        public async Task<string> StartSimpleMode(ILogger log, LineMessageReceiveJson json, string apiKey)
        {
            OpenAIAPI api = new OpenAIAPI(apiKey);

            ChatRequest chatRequest = new ChatRequest();
            chatRequest.MaxTokens = 100;
           

            var chat = api.Chat.CreateConversation();

            var prompt = json.events[0].message.text;
            chat.AppendUserInput(prompt);
            chat.Model = Model.ChatGPTTurbo;
            

            var result = await chat.GetResponseFromChatbotAsync();

            log.LogInformation($"ChatGPTMessageResult: {result}");
            return result;
        }


        private async Task<ChatResult> ExecChatGpt(ChatMessage[] messages)
        {
            _apiKey = GetApikey();
            OpenAIAPI api = new OpenAIAPI(_apiKey);
            var results = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                TopP = 0.1,
                MaxTokens = 2900,                
                Messages = messages
            }); ;

            // log.LogInformation($"StartEndpointModeResult: {results}");
            var reply = results.Choices[0].Message;
            return results;
        }

        private static string GetApikey()
        {
            KeyValueSetting keyValueSetting = new KeyValueSetting();
            return keyValueSetting.ChatGPTAPIKey;
        }

    }
}
