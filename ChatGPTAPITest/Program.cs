using ChatGPTAPITest.MessageClass;
using Newtonsoft.Json;
using System.Text;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using LineGPTAzureFunctions.ChatGPT;
using Microsoft.Extensions.Logging;
using OpenAI_API.Moderation;
using LineGPTAzureFunctions.Line;
using static ChatGPTAPITest.Program;

namespace ChatGPTAPITest
{
    internal class Program
    {

         async Task Main(string[] args, ILogger log)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;

            // 請替換成您自己的 API Key
            string apiKey = "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";
            string roleSetup = "你是一個很精通日文的AI，如果理解我的意思，第一句話就用日文回答我";

            // await HttpWayToChatGPT(apiKey);

            ChatGPTProcess chatGPTProcess = new ChatGPTProcess();
            List<ChatMessage> chatMessageList = new List<ChatMessage>();
            chatMessageList.Add(new ChatMessage(ChatMessageRole.User, roleSetup));

            ChatMessage[] messages = chatMessageList.ToArray();


            while (true)
            {
                ChatResult results = await chatGPTProcess.StartEndpointMode(log, apiKey, messages);

                // 印出回應
                var reply = results.Choices[0].Message;
                Console.WriteLine($"{reply.Role}: {reply.Content.Trim()}");


                // 設定下一個對話的 prompt
                chatMessageList.Add(new ChatMessage(reply.Role, reply.Content.Trim()));

                // 等待使用者輸入
                Console.Write("> ");

                string userInput = Console.ReadLine();
                chatMessageList.Add(new ChatMessage(reply.Role, userInput.Trim()));

            }
        }


        private static async Task HttpWayToChatGPT(string apiKey , string roleSetup)
        { 
            // 設定要使用的model 
            string model = "gpt-3.5-turbo";
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
                  temperature: 0.7,
                  max_tokens: 60,
                  top_p: 1,
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
                dynamic personData = JsonConvert.DeserializeObject(responseJson);
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
    }
}
