using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
using System.Media;
using Microsoft.CognitiveServices.Speech.Audio;
using Azure.Identity;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Configuration;
using Azure;

namespace LineGPTAzureFunctions.Audio
{
    public class Speech
    {

        // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
        //static string subscriptionKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        //static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");
        // static string IsEncrypted = ConfigurationManager.AppSettings["IsEncrypted"];

        static string speechKey = "c28b66aaf54c4ca283de7fc4d0dd92de";
        static string speechRegion = "japaneast";
        

        public Speech() {
             
        }

        public async Task<string> StartToText(string exampleAudioFilePath)
        {    
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            // 設置自適應辨識功能
            var autoDetectSourceLanguageConfig =
        AutoDetectSourceLanguageConfig.FromLanguages(
            new string[] { "en-US", "ja-JP", "zh-CN", "zh-TW", "zh-HK", "ko-KR" });


            using AudioConfig audioConfig = AudioConfig.FromWavFileInput(exampleAudioFilePath);
            using (SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig, autoDetectSourceLanguageConfig, audioConfig))
            {
                // 開始識別語音
                SpeechRecognitionResult result = recognizer.RecognizeOnceAsync().GetAwaiter().GetResult();

                // 取得識別的文字
                string recognizedText = result.Text;

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"辨識結果: {result.Text}");
                    // Console.WriteLine($"辨識語言: {result.Language}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine("無法辨識音頻");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"辨識取消，原因：{cancellation.Reason}");
                }

                // 輸出識別結果
                // Console.WriteLine("Recognized Text: " + recognizedText);
                return recognizedText;
            }
        }
         
          
    }
}
