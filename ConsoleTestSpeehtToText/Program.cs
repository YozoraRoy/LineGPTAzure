using LineGPTAzureFunctions.Audio;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ConsoleTestSpeehtToText
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string exampleAudioFilePath = await UseHttpFileToLocal();

            // string exampleAudioFilePath = await UserLocalExample();


            Speech speech = new Speech();
            await speech.StartToText(exampleAudioFilePath);
        }

        private static async Task<string> UseHttpFileToLocal()
        {
            // string wavUrl = "https://amitaro.net/download/voice/001_aisatsu/hajimemasite_01.wav"; // 替換為實際的 WAV 檔案網址 
            string wavUrl = "https://amitaro.net/download/voice/001_aisatsu/ohhayoo_01.wav";
            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            string savePath =  Path.Combine(projectDirectory, "file.wav");

            if (!File.Exists(savePath))
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        // 下載 WAV 檔案
                        byte[] wavBytes = await client.GetByteArrayAsync(wavUrl);

                        // 將檔案保存到本地
                        File.WriteAllBytes(savePath, wavBytes);

                        Console.WriteLine("WAV 檔案下載並保存成功。");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("發生錯誤: " + ex.Message);
                    }
                }
            }

            return savePath;

        }

        private static async Task<string> UserLocalExample()
        {

            // D:\工作相關\Slft專案\AGcore\LineGPTAzureFunctions\ConsoleTestSpeehtToText\bin\Debug\net6.0\time.wav
            // D:\工作相關\Slft專案\AGcore\LineGPTAzureFunctions\ConsoleTestSpeehtToText\time.wav

            string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            return Path.Combine(projectDirectory, "time.wav");

        }
    }
}