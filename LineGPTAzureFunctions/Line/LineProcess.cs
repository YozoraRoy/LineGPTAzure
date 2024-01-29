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
        // ILogger log;

        private readonly ILogger _logger;

        public LineProcess()
        {// 無作用
            _logger = LoggerHelper.GetLogger<LineProcess>();
        }

        public async Task ReplyAsync(string replyToken, string message, string lineUserName, string lineUserId)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _linechannelAccessToken);

                // Json? 2種套件?
                var response = await _httpClient.PostAsJsonAsync<LineTextReplyJson>(_lineMessagingApiUrl, new LineTextReplyJson()
                {
                    replyToken = replyToken,
                    messages = new List<Message>()
                    {
                        new Message()
                        {
                            type = "text",
                            text = message
                        }
                    }
                });
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                string msg = $"From [LineProcess HttpRequestException !!][User:{lineUserName} || UserID {lineUserId}] process exception error {ex.Message}";
                _logger.LogError($"User:{lineUserName} || UserID:{lineUserId} || {msg} || {ex}");
                string msgforLine = "Sorry, we are currently experiencing a network or server error and are working on it..";

                // 暫時不使用
                //await lineProcess.ReplyAsync(lineReplayToken, msgforLine);
                //_ = lineProcess.SendNotify(msg);

                // Clear All data by user id.
                // await ClearCosmosDB(log, lineUserId);

                throw ex;
            }
            catch (Exception ex)
            {

                throw;
            }
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

        public bool IsSingature(string signature, string text, string linechannelSecret)
        {

            var textBytes = Encoding.UTF8.GetBytes(text);
            var keyBytes = Encoding.UTF8.GetBytes(_linechannelSecret);

            if (!string.IsNullOrEmpty(linechannelSecret))
            {
                keyBytes = Encoding.UTF8.GetBytes(linechannelSecret);
            }

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(textBytes, 0, textBytes.Length);
                var hash64 = Convert.ToBase64String(hash);
                return signature == hash64;
            }
        }

        public async Task<UserProfile> GetUserProfile(string userId, string linechannelAccessToken)
        {
            if (!string.IsNullOrEmpty(linechannelAccessToken))
            {
                _linechannelAccessToken = linechannelAccessToken;
            }

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
