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

namespace LineGPTAzureFunctions.Line
{
    public class LineProcess
    {
        private readonly string _messagingApiUrl = "https://api.line.me/v2/bot/message/reply";
        private readonly string _linechannelAccessToken = @"jbwg5RbX/5A47Gg/xGgvMVE0WMFNjlpYzDrc2fyAGO07qikKownm3Wu4u7mrTbu15VgoqZgq/RfGo2RM0WlgHGpw/gSSa/BWNyGYx8tJaNioffVXTiGUBnjbsSNzijJEkAy9GA3w9XQKZCiCb7SLxwdB04t89/1O/w1cDnyilFU=";
        private readonly string _linechannelSecret = "1045973fc88d32d25dd2eb22586ddbed";
        private HttpClient _httpClient = new HttpClient();


        public async Task ReplyAsync(string replyToken, string message)
        {
            HttpClient _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _linechannelAccessToken);

            var response = await _httpClient.PostAsJsonAsync<LineTextReplyJson>(_messagingApiUrl, new LineTextReplyJson()
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
            //GET https://api.line.me/v2/bot/profile/{userId}
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.line.me/v2/bot/profile/{userId}"),
                Headers = {
                    { "Authorization", $"Bearer {_linechannelAccessToken}" },
                }
            };
            var result = await httpClient.SendAsync(httpRequestMessage);

            var content = await result.Content.ReadAsStringAsync();

            var profile = JsonConvert.DeserializeObject<UserProfile>(content);

            return profile;
        }


    }
}
