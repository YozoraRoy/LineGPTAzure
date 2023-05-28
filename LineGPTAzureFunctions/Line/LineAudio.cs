using LineGPTAzureFunctions.Audio;
using LineGPTAzureFunctions.Helper;
using LineGPTAzureFunctions.MessageClass;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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
        string lineChannelAccessToken = keyValueSetting.lineChannelAccessToken;

        private ILogger log;

        public LineAudio() { }

        public LineAudio(ILogger log)
        {
            this.log = log;
        }


        public async Task<string> ProcessWithAzureForSteam(string messageId)
        {
            try
            {
                string resutle = string.Empty;

                byte[] bytesResult = await ReadAudioFile(messageId, lineChannelAccessToken);

                MemoryStream memoryStream = new MemoryStream(bytesResult);

                // Azure cognitive speech  
                Speech speech = new Speech();

                using (var m4aStream = new MemoryStream(bytesResult))
                {
                    using (var reader = new StreamMediaFoundationReader(m4aStream))
                    {

                        //   NAudio.Wave.WaveStream to MemoryStream
                        MemoryStream wavStream = new MemoryStream();
                        using (WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(reader))
                        {
                            waveStream.CopyTo(wavStream);
                        }
                        wavStream.Position = 0;
                        log.LogInformation($"wavStream-Success");
                        resutle = await speech.StreamToText(wavStream);
                    }
                }
                 
                return resutle;

            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
                log.LogError(errormsg);
                throw;
            }
        }


        public async Task<string> ProcessWithAzureForSave(string messageId)
        {
            try
            {
                string resutle = string.Empty;

                byte[] bytesResult = await ReadAudioFile(messageId, lineChannelAccessToken);

                // log path:
                string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
                log.LogInformation($"projectDirectory:{projectDirectory}");

                string projectDirectoryUser = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "\\UserAudio\\"));
                log.LogInformation($"projectDirectoryUser:{projectDirectoryUser}");
                string m4aFilePath = Path.Combine(projectDirectoryUser, $"{messageId}.m4a");
                string wavFilePath = Path.Combine(projectDirectoryUser, $"{messageId}.wav");

                AudioConverter audioConverter = new AudioConverter();
                audioConverter.ByteToM4AToWAV(bytesResult, m4aFilePath, wavFilePath);

                // Azure cognitive speech  
                Speech speech = new Speech();
                resutle = await speech.FileToText(wavFilePath);

                return resutle;

            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
                log.LogError(errormsg);
                throw;
            }
        }


        public async Task<byte[]> ReadAudioFile(string messageId, string accessToken)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    string downloadUrl = $"{lineMessagingApiContentUrl}{messageId}/content";
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var response = await httpClient.GetAsync(downloadUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] audioData = await response.Content.ReadAsByteArrayAsync();
                        return audioData;
                    }
                    else
                    {
                        string errorMsg = $"Failed to download audio. Status Code: {response.StatusCode}";
                        Console.WriteLine(errorMsg);
                        log.LogError(errorMsg);
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return null;
            }

        }
    }



}
