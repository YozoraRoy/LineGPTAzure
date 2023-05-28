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
using LineGPTAzureFunctions.Helper;
using Microsoft.Extensions.Logging;

namespace LineGPTAzureFunctions.Audio
{
    public class Speech
    {
        private ILogger log;
        static KeyValueSetting keyValueSetting = new KeyValueSetting();
        string speechKey = keyValueSetting.speechKey;
        string speechRegion = keyValueSetting.speechRegion;

        public Speech()
        { }

        public Speech(ILogger log)
        {
            this.log = log;
        }

        public async Task<string> StartToText(string exampleAudioFilePath)
        {
            try
            {
                var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                var autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(
                new string[] { "ja-JP", "en-US", "zh-TW" });

                using AudioConfig audioConfig = AudioConfig.FromWavFileInput(exampleAudioFilePath);
                using (SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig, autoDetectSourceLanguageConfig, audioConfig))
                {
                    SpeechRecognitionResult result = await recognizer.RecognizeOnceAsync();

                    string recognizedText = result.Text;
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        recognizedText = result.Text;
                    }
                    else if (result.Reason == ResultReason.NoMatch)
                    {
                        recognizedText = "NoMatch";
                        log.LogInformation(recognizedText);

                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = CancellationDetails.FromResult(result);
                        recognizedText = $"Canceled，Reason：{cancellation.Reason}";
                        log.LogInformation(recognizedText);
                    }

                    return recognizedText;
                }
            }
            catch (Exception ex)
            {
                string message = $"Speech-StartToText-{ex.Message}";

                log.LogError(message);
                throw;
            }

        }
    }
}
