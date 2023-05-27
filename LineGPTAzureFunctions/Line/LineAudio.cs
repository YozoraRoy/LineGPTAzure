using LineGPTAzureFunctions.Helper;
using LineGPTAzureFunctions.MessageClass;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.Line
{
    public class LineAudio
    {
        static KeyValueSetting keyValueSetting = new KeyValueSetting();
        string lineMessagingApiContentUrl = keyValueSetting.lineMessagingApiContentUrl;
        private ILogger log;

        public LineAudio(ILogger log)
        {
            this.log = log;
        }


        public async Task<byte[]> ReadAudioFile(string messageId, string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                // string downloadUrl = $"https://api-data.line.me/v2/bot/message/{messageId}/content";

                string downloadUrl = $"{lineMessagingApiContentUrl}{messageId}/content";


                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);


                var response = await httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();


                byte[] fileData = await response.Content.ReadAsByteArrayAsync();
                return fileData;
            }
        }
    }



}
