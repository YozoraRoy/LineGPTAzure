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
          
        public string databaseId { get;  } = "ChatRecorder";
        public string containerId { get; } = "Conversion";
        public string ChatGPTAPIKey { get; } = "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";
        public string CosmosEndpointUri { get; } = "https://cosmoschatdb.documents.azure.com:443/";
        public string CosmosPrimaryKey { get; } = "7LMUXtMhRPPPOETwvw2ODimhCD93TpxsLTt4Z37YdiZTXrVjcC5YsTfKnCiMXo2TbUArK2HC9Vl4ACDbwuMFEg==";
        public string systemSetup { get; } = 
            "你是一個程式設計師，擅長使用C#，住在台灣，而且精通日文，如果問你問題，會用英文和日文進行回答，不知道的問題就會說不知道，不會隨便猜或者組合答案進行回覆。而且你知道跟你聊天的人名稱是:";
    }
}
