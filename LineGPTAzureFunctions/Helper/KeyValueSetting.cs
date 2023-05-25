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


        // Line
        public string lineNotifacationToken { get; } = "mzqlxldyXfzL7PLQQZXhUJqKpsZIZVCfnsWONMcbp2o";
        public string lineMessagingApiUrl { get; } = "https://api.line.me/v2/bot/message/reply";
        // GET https://api.line.me/v2/bot/profile/{userId}
        public string lineRequestUrl { get; } = "https://api.line.me/v2/bot/profile/";
        public string lineNotifyUrl { get; } = "https://notify-api.line.me/api/notify";

        public string linechannelAccessToken { get; } = @"jbwg5RbX/5A47Gg/xGgvMVE0WMFNjlpYzDrc2fyAGO07qikKownm3Wu4u7mrTbu15VgoqZgq/RfGo2RM0WlgHGpw/gSSa/BWNyGYx8tJaNioffVXTiGUBnjbsSNzijJEkAy9GA3w9XQKZCiCb7SLxwdB04t89/1O/w1cDnyilFU=";
        public string linechannelSecret { get; } = "1045973fc88d32d25dd2eb22586ddbed";

        // CosmosDB
        public string databaseId { get;  } = "ChatRecorder";
        public string containerId { get; } = "Conversion";
        public string ChatGPTAPIKey { get; } = "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";
        public string CosmosEndpointUri { get; } = "https://cosmoschatdb.documents.azure.com:443/";
        public string CosmosPrimaryKey { get; } = "7LMUXtMhRPPPOETwvw2ODimhCD93TpxsLTt4Z37YdiZTXrVjcC5YsTfKnCiMXo2TbUArK2HC9Vl4ACDbwuMFEg==";
        
        // chartGPT
        public string systemSetup { get; } =
            "你是一個日本語教師和同時也是中文老師，住在日本，之前也住過台灣，如果問你問題，會用英文和日文進行回答，不知道的問題就會說不知道，不會隨便猜或者組合答案進行回覆。而且你知道跟你聊天的人是:";
    }
}
