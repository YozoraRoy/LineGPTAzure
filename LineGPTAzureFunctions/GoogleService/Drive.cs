using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using Line.Messaging;
using OpenAI_API.Embedding;
using System.Threading.Tasks;
using LineGPTAzureFunctions.Helper;

namespace LineGPTAzureFunctions.GoogleService
{
    public class Drive
    {

        public ILogger _logger;
        KeyValueSetting keyValueSetting = new KeyValueSetting();
        public Drive(ILogger logger)
        {
            _logger = logger;
        }


        public async Task<string> Created(MemoryStream memoryStream, string lineUserName, string lineUserId)
        {
           
            string result = string.Empty;

            try
            {

                 string serviceAccountKeyJJson = keyValueSetting.googleDriveServiceAccountJson;

                // 設定使用者的權限範圍
                string[] scopes = { DriveService.Scope.Drive };

                 
                // 使用 JsonCredentialParameters 建立 Google 憑證
                var credential = GoogleCredential.FromJson(serviceAccountKeyJJson)
               .CreateScoped(scopes)
               .UnderlyingCredential as ServiceAccountCredential;


                // 建立Google Drive服務
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "DriveAccount",
                });


                // 指定父資料夾名稱
                string parentFolderName = "ImageForOpenAI";


                string fileName = $"{lineUserName}_{lineUserId}_{DateTime.Now.ToString("yyyymmddHHmmssff")}.jpg";

                _logger.LogInformation($"fileName: OK {fileName}");

                // 在Google Drive中建立一個檔案
                var fileId = CreateFile(_logger,service, parentFolderName, fileName, memoryStream);

                // 將檔案設為公開連結
                SetFilePermission(service, fileId);

                _logger.LogInformation($"SetFilePermission: OK");

                var filePublicLink = GetFilePublicLink(service, fileId);
               
                _logger.LogInformation($"File created successfully. Public link: {filePublicLink}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }

         

            return result;
        }


        private  string CreateFile(ILogger _logger,DriveService service, string parentFolderName, string fileName, MemoryStream contentStream)
        {
            string result = string.Empty;

            // 查詢父資料夾的ID
            var folderQuery = $"name='{parentFolderName}' and mimeType='application/vnd.google-apps.folder'";
            var folderListRequest = service.Files.List();
            folderListRequest.Q = folderQuery;

            try
            {
                var folderList = folderListRequest.Execute();

                var parentFolderId = folderList.Files.Select(f => f.Id)
                                                    .FirstOrDefault();

                _logger.LogInformation($"parentFolderId: OK {parentFolderId}");

                if (parentFolderId != null)
                {
                    // 在指定的父資料夾中建立檔案
                    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = fileName,
                        Parents = new List<string> { parentFolderId }
                    };

                    _logger.LogInformation($"fileMetadata: OK");

                    // 使用 CreateMediaUpload 來上傳檔案內容
                    contentStream.Position = 0; // MemoryStream のポジションをリセット
                    var request = service.Files.Create(fileMetadata, contentStream, "image/jpeg");
                    request.Fields = "id";

                    _logger.LogInformation($"request Files.Create: OK");

                    // 註冊上傳進度事件
                    request.ProgressChanged += (IUploadProgress progress) =>
                    {
                        switch (progress.Status)
                        {
                            case UploadStatus.Uploading:
                                string upliading = $"Uploading: {progress.BytesSent} bytes sent.";
                                _logger.LogInformation(upliading);
                                Console.WriteLine(upliading);
                                break;
                            case UploadStatus.Completed:

                                string upliadcompleted = $"Upload completed.";
                                _logger.LogInformation(upliadcompleted);
                                Console.WriteLine(upliadcompleted);
                                break;
                            case UploadStatus.Failed:
                                string upliadfailed = $"Upload failed: {progress.Exception.Message}";
                                _logger.LogInformation(upliadfailed);
                                Console.WriteLine();
                                break;
                        }
                    };

                    _logger.LogInformation($"IUploadProgress: OK");

                    // 開始上傳
                    var mediaUpload = request.Upload();
                    if (mediaUpload.Exception != null)
                    {
                        string ExceptionString = $"Upload failed: {mediaUpload.Exception.Message}";
                        _logger.LogError(ExceptionString);
                        Console.WriteLine(ExceptionString);
                    }
                    else
                    {
                        var file = request.ResponseBody;
                        // 印新檔案的 ID
                        Console.WriteLine($"File ID: {file.Id}");
                        result = file.Id;
                        _logger.LogInformation($"fileId:{result}--:OK");
                    }

                }
                else
                {
                    string notFound = $"Parent folder '{parentFolderName}' not found.";
                    _logger.LogError(notFound);
                    Console.WriteLine(notFound);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"ex!!! : {ex.Message}");
                throw ex;
            }

            return result;

        }

        private static void SetFilePermission(DriveService service, string fileId)
        {
            // 設定檔案的權限為公開連結
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };

            service.Permissions.Create(permission, fileId).Execute();
        }

        // 無效
        private static string GetFilePublicLink(DriveService service, string fileId)
        {
            try
            {
                var file = service.Files.Get(fileId).Execute();

                if (file != null && !string.IsNullOrEmpty(file.WebViewLink))
                {
                    return file.WebViewLink;
                }
                else
                {
                    Console.WriteLine($"File with ID '{fileId}' not found or doesn't have a public link.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting file details: {ex.Message}");
                return null;
            }
        }

    }
}
