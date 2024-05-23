using ConsoleTest.MessageClass;
using ConsoleTestOPENAI.GPTs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using LineGPTAzureFunctions.ChatGPT;
using LineGPTAzureFunctions.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Net;
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

            // await GoogleDrive().ConfigureAwait(false);
        }

        #region "Google雲端硬碟"

        public static async Task GoogleDrive()
        {
            // 設定服務帳號 JSON 檔案的路徑
            string serviceAccountKeyFilePath = "D:\\googleyozoraroyServiceAccount.json";


            //string serviceAccountKeyJJson = @"";

            // 設定使用者的權限範圍
            string[] scopes = { DriveService.Scope.Drive };

            //// 載入服務帳號 JSON 金鑰
            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountKeyFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(scopes);
            }
             

            // 使用 JsonCredentialParameters 建立 Google 憑證
            // var credential = GoogleCredential.FromJson(serviceAccountKeyJJson)
            //.CreateScoped(scopes)
            //.UnderlyingCredential as ServiceAccountCredential;


            // 建立Google Drive服務
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DriveAccount",
            });


            // 指定父資料夾名稱
            string parentFolderName = "ImageForOpenAI";

            // 在Google Drive中建立一個檔案
            var fileId = CreateFile(service, parentFolderName, "Hello.txt", "Hello, World!");

            // 將檔案設為公開連結
            SetFilePermission(service, fileId);

            var filePublicLink = GetFilePublicLink(service, fileId);
            Console.WriteLine($"File created successfully. Public link: {filePublicLink}");
        }

        private static string CreateFile(DriveService service, string parentFolderName, string fileName, string content)
        {
            string result = string.Empty;

            // 查詢父資料夾的ID
            var folderQuery = $"name='{parentFolderName}' and mimeType='application/vnd.google-apps.folder'";
            var folderListRequest = service.Files.List();
            folderListRequest.Q = folderQuery;

            try
            {
                var folderList = folderListRequest.Execute();

                var parentFolderId = folderList.Files.Select(f => f.Id)
                                                    .FirstOrDefault();

                if (parentFolderId != null)
                {
                    // 在指定的父資料夾中建立檔案
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = fileName,
                        Parents = new List<string> { parentFolderId }
                    };

                    var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

                    FilesResource.CreateMediaUpload request;
                    using (stream)
                    {
                        request = service.Files.Create(fileMetadata, stream, "text/plain");
                        request.Upload();
                    }

                    var file = request.ResponseBody;
                    Console.WriteLine($"File ID: {file.Id}");

                    result = file.Id;

                }
                else
                {
                    Console.WriteLine($"Parent folder '{parentFolderName}' not found.");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return result;

        }

        private static void SetFilePermission(DriveService service, string fileId)
        {
            // 設定檔案的權限為公開連結
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };

            service.Permissions.Create(permission, fileId).Execute();
        }


        private static string GetFilePublicLink(DriveService service, string fileId)
        {
            try
            {
                var file = service.Files.Get(fileId).Execute();

                if (file != null && !string.IsNullOrEmpty(file.WebViewLink))
                {
                    return file.WebViewLink;
                }
                else
                {
                    Console.WriteLine($"File with ID '{fileId}' not found or doesn't have a public link.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting file details: {ex.Message}");
                return null;
            }
        }

        #endregion

         
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

     
        //--------------------------

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