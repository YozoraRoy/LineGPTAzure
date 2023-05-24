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
        static KeyValueSetting keyValueSetting = new KeyValueSetting();
        string systemSetup = keyValueSetting.systemSetup; 

        public List<ChatMessage> NewConversationMessage(string userId, string userDisplayName, string usermeeage, List<ChatMessage> chatMessageList)
        {
            string mergemsg = string.Format("{0}{1}", systemSetup, userDisplayName);
            chatMessageList.Add(new ChatMessage(ChatMessageRole.System, mergemsg)); // Set CharGpt Role
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, $"Hello  ! {userDisplayName}"));
            chatMessageList.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
            return chatMessageList;
        }
    }
}
