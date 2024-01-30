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
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using static Google.Apis.Requests.BatchRequest;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;

namespace LineGPTAzureFunctions.ChatGPT
{
    public class AIImager
    {
        private KeyValueSetting _keyValueSetting;

        private readonly ILogger _logger;

        public AIImager(ILogger logger)
        {

            _logger = logger;
            _keyValueSetting = new KeyValueSetting();
        }

        public async Task<string> ViewerAsync(MemoryStream imageData, string command)
        {
            string result = string.Empty;

            // OpenAI API Key
            string apiKey = _keyValueSetting.gptOpenAIKey001;

            // Image data in MemoryStream
            // MemoryStream imageData = GetImageData();

            _logger.LogInformation("ViewerAsync OK");

            // Getting the base64 string
            string base64Image = EncodeImage(imageData);

            _logger.LogInformation("base64Image OK");

            try
            {
           
                // API endpoint
                string apiUrl = "https://api.openai.com/v1/chat/completions";

                _logger.LogInformation("apiUrl OK");

                using var client = new HttpClient();
                
                _logger.LogInformation("client OK");

                // Set headers
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                  
                _logger.LogInformation("DefaultRequestHeaders OK");

                // Prepare payload
                var payload = new
                {
                    model = "gpt-4-vision-preview",
                    messages = new[]
                      {
                            new
                            {
                                role = "user",
                                content = new object[]
                                {
                                    new
                                    {
                                        type = "text",
                                        text = command
                                    },
                                    new
                                    {
                                        type = "image_url",
                                        image_url = new
                                        {
                                            url = $"data:image/jpeg;base64,{base64Image}"
                                        }
                                    }
                                }
                            }
                        },
                    max_tokens = 500
                };

                _logger.LogInformation("payload OK");


                // Convert payload to JSON
                string jsonPayload = JsonConvert.SerializeObject(payload);

                _logger.LogInformation("jsonPayload OK");

                // Post request
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                _logger.LogInformation("response  OK");

                // Read and print the response
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"responseContent OK : {responseContent}");

                // Deserialize JSON response
                ChatCompletion chatCompletion = JsonConvert.DeserializeObject<ChatCompletion>(responseContent);

                // Access the required information
                int totalTokens = chatCompletion.Usage.TotalTokens;
                _logger.LogInformation(DecodeUnicode(chatCompletion.Choices[0]?.Message?.Content));

                string resultContent = DecodeUnicode(chatCompletion.Choices[0]?.Message?.Content);
                 
                result = Regex.Replace(resultContent, @"\s", "");
             
            }
            catch (Exception ex)
            {
                _logger.LogError($"ex!!!{ex.Message}");
                throw ex;
            }

            return result;

        }


        // Function to encode the image from MemoryStream
        private string EncodeImage(MemoryStream imageData)
        {
            byte[] imageBytes = imageData.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        // 解碼 Unicode 字串的方法
        private string DecodeUnicode(string unicodeString)
        {
            string str = System.Uri.UnescapeDataString(unicodeString);
            return str;
        }


        public class ChatCompletion
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long Created { get; set; }
            public string Model { get; set; }
            public Usage Usage { get; set; }
            public Choice[] Choices { get; set; }
        }

        public class Usage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        public class Choice
        {
            public Message Message { get; set; }
            public string FinishReason { get; set; }
            public int Index { get; set; }
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

    }
}
