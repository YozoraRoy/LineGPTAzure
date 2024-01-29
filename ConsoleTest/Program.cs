using ConsoleTest.MessageClass;
using ConsoleTestOPENAI.GPTs;
using LineGPTAzureFunctions.ChatGPT;
using LineGPTAzureFunctions.Helper;
using Microsoft.Extensions.Logging;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace ConsoleTest
{
    /// <summary>
    /// Test Open AI Console
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 建立 ILoggerFactory
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            // 建立 ILogger
            ILogger log = loggerFactory.CreateLogger<Program>();

            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            KeyValueSetting keyValueSetting = new KeyValueSetting();

            // 你的OpenAI API金鑰
            string apiKey = keyValueSetting.gptOpenAIKey002;
            string assistantID = keyValueSetting.assistantId002;
            string assistantName = keyValueSetting.assistantName002;

            // ConsoleTestOPENAI.GPTs.Assistant assistant = new ConsoleTestOPENAI.GPTs.Assistant();
            await ConsoleTestOPENAI.GPTs.Assistant.PriceHelper(apiKey, assistantID, assistantName).ConfigureAwait(false);
        }


        private static async Task ChatCreated(ILogger log)
        {
            // Replace this to your API Key
            string apiKey = string.Empty;

            ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
            List<ChatMessage> chatMessageList = new List<ChatMessage>();
            string systemSetup =
                @"你是一個很精通英文和日文的AI，而且擅長用C#撰寫程式語言，如果問你問題，會用英文和日文進行回答。";
            // + "你知道使用者的ID是ABC1234567，如果有人問他的使用者ID，就回答他ABC1234567。"
            // + "從現在起如果有人跟你問'自訂角色'或者自訂GPT角色等等意思的話，就回傳https://chatwithroy.azurewebsites.net/<使用者ID>";
            chatMessageList.Add(new ChatMessage(ChatMessageRole.System, systemSetup));

            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, "你知道使用者的ID是ABC1234567，如果有人問你他的使用者ID，就回答他。，妳好，羅伊!!"));

            ChatMessage[] messages = chatMessageList.ToArray();
            chatMessageList.Clear();

            // IConfiguration configuration = new ConfigurationBuilder()
            //  .AddJsonFile("appsettings.json")
            //  .Build();

            List<ChatMessage> chat_message_list = new();

            try
            {
                while (true)
                {

                    ChatResult results = await chatGPTProcess.StartEndpointMode(log, messages);

                    // 印出回應
                    var reply = results.Choices[0].Message;
                    Console.WriteLine($"{reply.Role}: {reply.Content.Trim()}");

                    // 設定下一個對話的 prompt
                    chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, reply.Content.Trim()));

                    // 等待使用者輸入
                    Console.Write("> ");

                    string userInput = Console.ReadLine();
                    chatMessageList.Add(new ChatMessage(ChatMessageRole.User, userInput.Trim()));
                    messages = chatMessageList.ToArray();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // un used
        private static async Task HttpWayToChatGPT(string apiKey, string roleSetup)
        {
            // 設定要使用的model 
            string model = "";
            string userrole = "user";
            string systemrole = "system";


            // 設定初始對話
            var defultChatGPTMsg = new List<ChatGPTMsg> { new ChatGPTMsg { role = userrole, content = roleSetup } };

            // 建立 HttpClient 物件
            HttpClient httpClient = new HttpClient();

            // 加入 API Key 到 HTTP 請求標頭中
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            while (true)
            {
                var messagesss = defultChatGPTMsg;

                // 設定要發送的資料
                var requestDatas = new RequestData(
                  model: model,
                  messages: messagesss,
                  temperature: 0.1,
                  max_tokens: 3500,
                  top_p: 0.1,
                  frequency_penalty: 0,
                  presence_penalty: 0,
                  stop: "\n"
                  );

                string requestJson = System.Text.Json.JsonSerializer.Serialize(requestDatas);
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");


                // 發送 POST 請求
                var response = await httpClient.PostAsync($"https://api.openai.com/v1/chat/completions", content);

                // 讀取回應資料
                var responseJson = await response.Content.ReadAsStringAsync();

                // 解析回應資料
                dynamic personData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(responseJson) ?? "";
                dynamic message_id = personData.id;
                dynamic message = personData.choices[0].message.content;

                // 印出回應
                Console.WriteLine(message);

                // 設定下一個對話的 prompt

                messagesss.Add(new ChatGPTMsg { role = systemrole, content = message });

                // 等待使用者輸入
                Console.Write("> ");

                string userInput = Console.ReadLine();


                // 設定使用者輸入為下一個對話的 
                messagesss.Add(new ChatGPTMsg { role = userrole, content = userInput });
            }
        }


        public class ThreadResponse
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long CreatedAt { get; set; }
            public object Metadata { get; set; }
        }


        public class ThreadRun
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long CreatedAt { get; set; }
            public string AssistantId { get; set; }
            public string ThreadId { get; set; }
            public string Status { get; set; }
            public long StartedAt { get; set; }
            public long? ExpiresAt { get; set; }
            public long? CancelledAt { get; set; }
            public long? FailedAt { get; set; }
            public long CompletedAt { get; set; }
            public string LastError { get; set; }
            public string Model { get; set; }
            public string Instructions { get; set; }
            public List<Tool> Tools { get; set; }
            public List<string> FileIds { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

        public class Tool
        {
            public string Type { get; set; }
        }

    }
}