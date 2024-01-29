using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using LineGPTAzureFunctions.Helper;
using LineGPTAzureFunctions.Models.OpenAI;

namespace LineGPTAzureFunctions.ChatGPT
{
    public class Assistant
    {
        private KeyValueSetting _keyValueSetting;

        private readonly ILogger _logger;

        public Assistant(ILogger logger)
        {

            _logger = logger;
            _keyValueSetting = new KeyValueSetting();
        }

        public Assistant()
        {
            _logger = LoggerHelper.GetLogger<Assistant>();
            _keyValueSetting = new KeyValueSetting();
        }

        public async Task<ListMessagesResponse> JPTeacherHelper(string userInput)
        {
            ListMessagesResponse result = new ListMessagesResponse();

            string assistantApiKey = _keyValueSetting.gptOpenAIKey001;

            // 助理id
            var assistantId = _keyValueSetting.assistantId001;

            // 將 assistantId 記錄下來
            // 要查看有那些 Assistants 可以呼叫 ListAssistants()
            // await ListAssistants();

            //var tools = new List<object> { new { type = "retrieval" } };
            //var model = "gpt-4-1106-preview";

            // 上傳檔案id
            // string file1Id = "file-";
            // var file_ids = new List<string> { file1Id };
            // 助理名稱
            // var assistantName = "MIBY";
            // await CreateAssistant(instructions, assistantName, tools, model, file_ids, apiKey);       
            // await ListAssistants();

            // 為使用者建立 Thread
            var thread = await CreateThread(assistantApiKey).ConfigureAwait(false);

            //發送問題
            await AddMessage(thread.Id, userInput, assistantApiKey).ConfigureAwait(false);

            ThreadRun runObj = await RunThread(assistantId, thread.Id, assistantApiKey).ConfigureAwait(false);

            // 將 runId 記錄下來
            // var run1Id = "run_B62wmGcFQyBllyRDGhNXfTBG";

            Console.Write($"GetRun : runObjId={runObj.Id}");
            _logger.LogInformation("GetRun : runObjId={runObj.Id}");

            // Loop 等到 Run 的狀態為 completed 才離開
            bool runObjResult = await GetRun(runObj.Id, thread.Id, assistantApiKey);

            if (runObjResult)
            {
                Console.Write("ListMessages");
                _logger.LogInformation("ListMessages");
                var jsonString = await ListMessages(thread.Id, assistantApiKey).ConfigureAwait(false);

                result = Newtonsoft.Json.JsonConvert.DeserializeObject<ListMessagesResponse>(jsonString);

                Console.Write("DeleteThread");
                _logger.LogInformation("DeleteThread");
                await DeleteThread(thread.Id, assistantApiKey).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<string> DeleteThread(string threadId, string apiKey)
        {
            string result = string.Empty;

            var url = $"https://api.openai.com/v1/threads/{threadId}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = await client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }

            return result;
        }

        private async Task<string> ListMessages(string threadId, string apiKey)
        {
            string result = string.Empty;

            var url = $"https://api.openai.com/v1/threads/{threadId}/messages?order=desc&limit=2";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine($"錯誤：{response.StatusCode} - {response.ReasonPhrase}");
                result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }

            return result;
        }

        private async Task<bool> GetRun(string runID, string threadId, string apiKey)
        {
            bool runObjectResult = false;
            while (runObjectResult == false)
            {

                var url = $"https://api.openai.com/v1/threads/{threadId}/runs/{runID}";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Console.WriteLine(responseBody);

                    var runObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreadRun>(responseBody);

                    if (runObject.Status == "completed")
                    {
                        runObjectResult = true;
                        return true;
                    }
                }
                else
                {
                    Thread.Sleep(1500);
                    await GetRun(runID, threadId, apiKey).ConfigureAwait(false);
                }
            }

            return runObjectResult;
        }

        private async Task<ThreadRun> RunThread(string assistantId, string threadId, string apiKey)
        {
            ThreadRun runObjectResult = new ThreadRun();

            var url = $"https://api.openai.com/v1/threads/{threadId}/runs";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                assistant_id = assistantId,

            });
            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseBody);

                var runObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreadRun>(responseBody);
                runObjectResult = runObject;
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }

            return runObjectResult;
        }

        private async Task AddMessage(string threadId, string question, string apiKey)
        {
            var url = $"https://api.openai.com/v1/threads/{threadId}/messages";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                role = "user",
                content = question
            });
            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        private async Task<ThreadResponse> CreateThread(string apiKey)
        {
            ThreadResponse resultThread = new ThreadResponse();

            var url = "https://api.openai.com/v1/threads";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            var response = await client.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine(responseBody);

                // 解析回傳的 JSON，取得 id
                var thread = Newtonsoft.Json.JsonConvert.DeserializeObject<ThreadResponse>(responseBody);
                resultThread = thread;
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }

            return resultThread;
        }

        private async Task CreateAssistant(string instructions, string name, List<object> tools, string model, List<string> file_ids, string apiKey)
        {
            var url = "https://api.openai.com/v1/assistants";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var jsonStr = JsonSerializer.Serialize(new
            {
                instructions,
                name,
                tools,
                model,
                file_ids
            });

            using var jsonContent = new StringContent(jsonStr,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"Err:{response.StatusCode}-{response.ReasonPhrase}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

        private async Task ListAssistants(string apiKey)
        {
            var url = "https://api.openai.com/v1/assistants?order=desc&limit=20";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"錯誤：{response.StatusCode} - {response.ReasonPhrase}");
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
        }

    }
}
