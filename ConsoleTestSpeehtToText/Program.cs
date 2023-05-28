using LineGPTAzureFunctions.Audio;
using LineGPTAzureFunctions.Helper;
using LineGPTAzureFunctions.Line;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConsoleTestSpeehtToText
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            KeyValueSetting keyValueSetting = new KeyValueSetting();

            //string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            //string savePath_wav = Path.Combine(projectDirectory, "file1.wav");
            //string savePath_m4a = Path.Combine(projectDirectory, "file1.m4a");

            //LineAudio lineAudio = new LineAudio();
            //byte[] lineResult = await lineAudio.ReadAudioFile("457020023098573158", keyValueSetting.lineChannelAccessToken);
            //await UseLineFileToLocal(lineResult, savePath_wav, savePath_m4a);

            Speech speech = new Speech();
            //var wavText = await speech.StartToText(savePath_wav);
             
            var spath1 = UserLocalExample("file1.wav");
            var spath2 = UserLocalExample("file.wav");
            var spath3 = UserLocalExample("time.wav");

            var wavText1 = await speech.StartToText(spath1);
            Console.WriteLine(wavText1);
            var wavText2 = await speech.StartToText(spath2);
            Console.WriteLine(wavText2);
            var wavText3 = await speech.StartToText(spath3);
            Console.WriteLine(wavText3);

        }

        private static async Task UseLineFileToLocal(byte[] lineResult , string savePath_wav, string savePath_m4a)
        {

            AudioConverter audioConverter = new AudioConverter();
             
            if (!File.Exists(savePath_m4a))
            {
                audioConverter.ConvertToM4A(lineResult, savePath_m4a);
            }

            if (!File.Exists(savePath_wav))
            {
                audioConverter.ConvertM4AToWAV(savePath_m4a, savePath_wav);
            }

        }

        private static async Task<string> UseHttpFileToLocal()
        {
            // string wavUrl = "https://amitaro.net/download/voice/001_aisatsu/hajimemasite_01.wav"; // 替換為實際的 WAV 檔案網址 
            string wavUrl = "https://amitaro.net/download/voice/001_aisatsu/ohhayoo_01.wav";
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            string savePath = Path.Combine(projectDirectory, "file.wav");

            if (!File.Exists(savePath))
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        byte[] wavBytes = await client.GetByteArrayAsync(wavUrl);

                        File.WriteAllBytes(savePath, wavBytes);

                        Console.WriteLine("WAV Download success。");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }

            return savePath;

        }

        private static string UserLocalExample(string fileFullName)
        {
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            return Path.Combine(projectDirectory, fileFullName);
        }
    }
}