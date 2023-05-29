using LineGPTAzureFunctions.MessageClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LineGPTAzureFunctions.Helper;
using Microsoft.Extensions.Logging;

namespace LineGPTAzureFunctions.Line
{
    public class LineProcess
    {
       static KeyValueSetting keyValueSetting = new KeyValueSetting();
        private string _lineMessagingApiUrl = keyValueSetting.lineMessagingApiUrl;
        private string _linechannelAccessToken = keyValueSetting.lineChannelAccessToken;
        private string _linechannelSecret = keyValueSetting.linechannelSecret;
        private string _lineRequestUrl = keyValueSetting.lineRequestUrl;
        private string _lineNotifyUrl = keyValueSetting.lineNotifyUrl;

        private HttpClient _httpClient = new HttpClient();
        ILogger log;
      

        public async Task ReplyAsync(string replyToken, string message)
        {

            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _linechannelAccessToken);

            var response = await _httpClient.PostAsJsonAsync<LineTextReplyJson>(_lineMessagingApiUrl, new LineTextReplyJson()
            {
                replyToken = replyToken,
                messages = new List<Message>()
        {
            new Message(){
                type = "text",
                text = message
            }
        }
            });
            response.EnsureSuccessStatusCode();
        }

        public bool IsSingature(string signature, string text)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            var keyBytes = Encoding.UTF8.GetBytes(_linechannelSecret);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(textBytes, 0, textBytes.Length);
                var hash64 = Convert.ToBase64String(hash);
                return signature == hash64;
            }
        }

        public async Task<UserProfile> GetUserProfile(string userId)
        {
            HttpClient httpClient = new HttpClient();
          
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_lineRequestUrl}{userId}"),
                Headers = {
                    { "Authorization", $"Bearer {_linechannelAccessToken}" },
                }
            };
            var result = await httpClient.SendAsync(httpRequestMessage);

            var content = await result.Content.ReadAsStringAsync();

            var profile = JsonConvert.DeserializeObject<UserProfile>(content);

            return profile;
        }

        public async Task SendNotify(string sendMsg)
        {
            KeyValueSetting keyValueSetting = new KeyValueSetting();
            string token = keyValueSetting.lineNotifacationToken;
            using (HttpClient client = new HttpClient())
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string> { { "message", "\r\n" + sendMsg } });
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage httpResponseMessage = await client.PostAsync($"{_lineNotifyUrl}", content);
                string result = await httpResponseMessage.Content.ReadAsStringAsync();
                // log.LogInformation($"SendNotifyResult : {result}");
                // _ = httpResponseMessage;
                //return result;
            }
        }
    }
}
