using Azure.Core;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using LineGPTAzureFunctions.GoogleService;
using LineGPTAzureFunctions.ChatGPT;

public class LineContent
{
   public ILogger _logger;

    public LineContent(ILogger logger)
    {
        _logger = logger;
    }


    public async Task<string> GetImageToDrive(string channelAccessToken, string messageId, string lineUserName, string lineUserId)
    {
        string result = string.Empty;

        // API 端點 URL
        string apiUrl = $"https://api-data.line.me/v2/bot/message/{messageId}/content";

        _logger.LogInformation($"messageId:{messageId}");

        try
        {

            using (HttpClient client = new HttpClient())
            {
                // 設定請求標頭
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {channelAccessToken}");

                // 發送 GET 請求
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                _logger.LogInformation($"response:{response.Content}");

                // 檢查回應狀態碼
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"IsSuccessStatusCode:{response.IsSuccessStatusCode}");
                    // 處理成功回應，可以從 response.Content 中讀取內容
                    string base64Content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    result = base64Content;

                    _logger.LogInformation($"base64Content: OK");
                    // 將二進制轉換為圖片
                    MemoryStream imageMs = await ConvertByteArrayToImage(response.Content.ReadAsByteArrayAsync());

                    _logger.LogInformation($"MemoryStream: OK");

                    // 非同步備份到Google 雲端硬碟
                    Drive gDrive = new Drive(_logger);
                    await gDrive.Created(imageMs, lineUserName, lineUserId).ConfigureAwait(false);

                    // 讓Open AI 辨識
                     AIImager aiImager = new AIImager(_logger);
                     var ViewerResult = await aiImager.ViewerAsync(imageMs, "What's in this image? Please answer in Japanese and Tradionnal Chinese.").ConfigureAwait(false);
                     result = ViewerResult;

                    imageMs.Dispose();
                }
                else
                {
             
                    string errorMsg = $"錯誤：{response.StatusCode}";
                    _logger.LogError(errorMsg);
                    result = errorMsg;
                    // 處理錯誤回應
                    Console.WriteLine(errorMsg);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            string errorMsg = $"ex錯誤：{ex.Message}";
            result = errorMsg;
        }

        return result;

    }

    static async Task<MemoryStream> ConvertByteArrayToImage(Task<byte[]> byteArrayTask)
    {
        byte[] imageData = await byteArrayTask;

        //using (MemoryStream ms = new MemoryStream(imageData))
        //{
        //    return ms;
        //}

        MemoryStream ms = new MemoryStream(imageData);
        return ms;

    }
     
}