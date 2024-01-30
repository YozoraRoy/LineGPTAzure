using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LineGPTAzureFunctions.Helper;

public class LineMenuCustomization
{
    private static readonly HttpClient _httpClient = new HttpClient();
    static KeyValueSetting keyValueSetting = new KeyValueSetting();
    private static readonly string _channelAccessToken = keyValueSetting.linechannelAccessTokenRoyGPT;
    private string _richMenuId = keyValueSetting.richMenuId;

    public async Task<string> GetRichMenu()
    {
        try
        {

            string url = $"https://api.line.me/v2/bot/richmenu/{_richMenuId}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {_channelAccessToken}");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseContent;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<string> GetAllRichMenus()
    {
        try
        {
            string url = "https://api.line.me/v2/bot/richmenu/list";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {_channelAccessToken}");

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseContent;
            }
            else
            {                
                return null;
            }
        }
        catch (Exception ex)
        {            
            return null;
        }
    }

    public async Task<string> CreatedRichMenu()
    {

        string reslut = string.Empty;

        // 設定 Channel Access Token
        string channelAccessToken = _channelAccessToken;

        // 建立圖文選單的 JSON 資料
        string richMenuJson = @"
            {
                ""size"": {
                    ""width"": 2500,
                    ""height"": 843
                },
                ""selected"": false,
                ""name"": ""richmenu-hotkey"",
                ""chatBarText"": ""Check more"",
                ""areas"": [
                    {
                        ""bounds"": {
                            ""x"": 0,
                            ""y"": 0,
                            ""width"": 1250,
                            ""height"": 843
                        },
                        ""action"": {
                            ""type"": ""message"",
                            ""text"": ""Please forget what we just said.""
                        }
                    },
                    {
                        ""bounds"": {
                            ""x"": 1250,
                            ""y"": 0,
                            ""width"": 1250,
                            ""height"": 843
                        },
                        ""action"": {
                            ""type"": ""uri"",
                            ""uri"": ""https://localhost:7104/Linepage/GPTDtaSet/""
                        }
                    }
                ]
            }
            ";
        
        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri("https://api.line.me/v2/bot/");
            
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {channelAccessToken}");

            var content = new StringContent(richMenuJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("richmenu", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Rich menu created successfully.");
                Console.WriteLine(responseContent);
                reslut = $"Rich menu created successfully. {responseContent}";
            }
            else
            {
                Console.WriteLine($"Error creating rich menu. Status code: {response.StatusCode}");

                reslut = $"\"Error creating rich menu. Status code: {response.StatusCode}/{responseContent}";
            }

        }

        Console.WriteLine(reslut);
        return reslut;     

    }


    public async Task<string> UploadRichMenuImage()
    {
        string imageUrl = "D:\\工作相關\\Slft專案\\AGcore\\LineGPTAzureFunctions\\ConsolerTestLineAPI\\richmenu001.png";



        string reslut = string.Empty;

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _channelAccessToken);

        byte[] imageBytes;
        using (var imageStream = new FileStream(imageUrl, FileMode.Open, FileAccess.Read))
        {
            using (var memoryStream = new MemoryStream())
            {
                await imageStream.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }
        }

        var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // 修改成相應的圖片類型，例如 "image/jpeg" 或 "image/gif" 等

        var uploadImageUrl = $"https://api-data.line.me/v2/bot/richmenu/{_richMenuId}/content";
        var response = await httpClient.PostAsync(uploadImageUrl, content);

        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            reslut = $"Failed to upload rich menu image. Status code: {response.StatusCode}";
            // 處理上傳圖片失敗的情況
            Console.WriteLine(reslut);

        }
        else
        {
            reslut = "Rich menu image uploaded successfully.";
            Console.WriteLine(reslut);
        }

        return reslut;

    }

    public async Task<string> AssignRichMenu(bool isAssign)
    {
        string reslut = string.Empty;

        string userId = "U6fa80bda7dcb81a8a51d926ff36d832b";


        string linkUrl = string.Empty;

        if (isAssign)
        {
            linkUrl = $"https://api.line.me/v2/bot/user/{userId}/richmenu/{_richMenuId}";

        }
        else
        {
            linkUrl = $"https://api.line.me/v2/bot/user/{userId}/richmenu/";
        }


        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_channelAccessToken}");
            HttpResponseMessage response;
            if (isAssign)
            {
                response = await client.PostAsync(linkUrl, new StringContent("", Encoding.UTF8, "application/json"));
            }
            else
            {
                response = await client.DeleteAsync(linkUrl);
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                reslut = "Rich Menu linked to user successfully.";
                Console.WriteLine(reslut);
            }
            else
            {
                reslut = $"Response: {response.StatusCode} - {response.ReasonPhrase}";
                Console.WriteLine("Failed to link Rich Menu to user.");
                Console.WriteLine(reslut);
            }
        }

        return reslut;
    }


}