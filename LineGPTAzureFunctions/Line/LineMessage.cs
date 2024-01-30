using LineGPTAzureFunctions.Helper;
using System;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Line
{
    public class LineMessage
    {
        private KeyValueSetting _keyValueSetting;

        private readonly  ILogger _logger;

        public LineMessage(ILogger logger)
        {

            _logger = logger;
            _keyValueSetting = new KeyValueSetting();
        }

        public async Task<string> AddMsg(string userId, string msg)
        {
            string result = string.Empty;

            // LINE Messaging API 的 Channel Access Token
            string channelAccessToken = _keyValueSetting.linechannelAccessTokenRoyGPT; // 要替換成您的 Channel Access Token

            _logger.LogInformation("AddMsg OK");

            // 建立 HttpClient
            using (HttpClient client = new HttpClient())
            {
                 
                // 設定 Authorization Header
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {channelAccessToken}");


                _logger.LogInformation("client ready content OK");

                // 準備推送的內容
                string pushContent = $@"{{
                ""to"": ""{userId}"",
                ""messages"": [
                    {{
                        ""type"": ""text"",
                        ""text"": ""{msg}""
                        }}                   
                    ]
                }}";

                // 設定內容型別為 application/json
                StringContent content = new StringContent(pushContent, Encoding.UTF8, "application/json");
                _logger.LogInformation("Message ready content OK");


                try
                {
                    // 發送 POST 請求進行推送
                    HttpResponseMessage response = await client.PostAsync("https://api.line.me/v2/bot/message/push", content);

                    // 處理回應
                    if (response.IsSuccessStatusCode)
                    {
                        // 推送成功
                        _logger.LogInformation("Message pushed successfully!");

                        result = "Message pushed successfully!";
                    }
                    else
                    {
                        // 推送失敗，顯示錯誤訊息
                        string errorMessage = await response.Content.ReadAsStringAsync();

                        result = $"Failed to push message. Error: {errorMessage}";
                        _logger.LogError(result);

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"{ex.Message}");
                    throw ex;
                }

              
            }

            return result; 

        }

    }
}
