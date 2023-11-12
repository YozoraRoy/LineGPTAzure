using LineGPTAzureFunctions.DB;
using Microsoft.Extensions.Logging;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Helper
{
    public class MessageMapping
    {
        private readonly ILogger _log;
        private readonly KeyValueSetting _keyValueSetting;
        
        string promptSetup = string.Empty;

        public MessageMapping()
        {          
            _keyValueSetting = new KeyValueSetting();
        }

        public MessageMapping(ILogger log)
        {
            _log = log;
            _keyValueSetting = new KeyValueSetting();
        }

        public async Task<List<ChatMessage>> NewConversationMessage(string userId, string userDisplayName, string usermeeage, List<ChatMessage> chatMessageList)
        {
            _log.LogInformation($"NewConversationMessage - userId: / {userId}");

            CosmosProcess cosmosProcess = new CosmosProcess(_log); 
            promptSetup = await cosmosProcess.QueryUserPromptWithDefaultAsync(userId);

            //promptSetup = _keyValueSetting.promptsetup;

            string mergemsg = string.Format("{0}{1}", promptSetup, userDisplayName);
            chatMessageList.Add(new ChatMessage(ChatMessageRole.System, mergemsg)); // Set CharGpt Role
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, $"Hello  ! {userDisplayName}"));
            chatMessageList.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
            return chatMessageList;
        }

        public async Task<List<ChatMessage>> NewConversationMessageWithoutLog(string userId, string userDisplayName, string usermeeage, List<ChatMessage> chatMessageList)
        {
            CosmosProcess cosmosProcess = new CosmosProcess();
            promptSetup = await cosmosProcess.QueryUserPromptWithDefaultAsync(userId);

            string mergemsg = string.Format("{0}{1}", promptSetup, userDisplayName);
            chatMessageList.Add(new ChatMessage(ChatMessageRole.System, mergemsg)); // Set CharGpt Role
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, $"Hello  ! {userDisplayName}"));
            chatMessageList.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
            return chatMessageList;
        }
    }
}
