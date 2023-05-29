using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Helper
{
    public class KeyValueSetting
    {
        public KeyValueSetting() { }


        // Line[Notifacation]
        public string lineNotifacationToken { get; } = "";

        public string lineMessagingApiUrl { get; } = "https://api.line.me/v2/bot/message/reply";
        public string lineMessagingApiContentUrl { get; } = "https://api-data.line.me/v2/bot/message/";
        // GET https://api.line.me/v2/bot/profile/{userId}
        public string lineRequestUrl { get; } = "https://api.line.me/v2/bot/profile/";
        public string lineNotifyUrl { get; } = "https://notify-api.line.me/api/notify";

        // -- Line Roybot
        public string lineChannelAccessToken { get; } = @"";
        public string linechannelSecret { get; } = "";
          
        //Azure CosmosDB
        public string databaseId { get;  } = "databaseId";
        public string containerId { get; } = "containerId";
        public string ChatGPTAPIKey { get; } = "sk-xxxx";
        public string CosmosEndpointUri { get; } = "https://cosmoschatdb.documents.azure.com:443/";
        public string CosmosPrimaryKey { get; } = "==";

        //Azure Speech
        public string speechKey { get; } = "";
        public string speechRegion { get; } = "";


        // chartGPT
        public string systemSetup { get; } =
            " You are a Japanese teacher and you know who is talking  to you:";
    }
}
