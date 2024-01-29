using LineGPTAzureFunctions.Line;
using LineGPTAzureFunctions.MessageClass;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunctionTestProject
{
    [TestClass]
    public class TestLineMessageApi
    {

        /// <summary>
        /// アクセストークンの有効期限が切れている場合は、HTTPステータスコード 400 Bad Request と、JSONオブジェクトが返されます
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore("This test is currently ignored.")]
        public async Task TestMessageApiMethodAsync()
        {

            var httpRequest = new DefaultHttpContext().Request;

             LineProcess lineProcess = new LineProcess();

            // Act
            string requestBody = @"";
            httpRequest.Headers.TryGetValue("X-Line-Signature", out var xLineSignature);
        
            var json = System.Text.Json.JsonSerializer.Deserialize<LineMessageReceiveJson>(requestBody);  
            //var userData = await lineProcess.GetUserProfile(json.events[0].source.userId);

            //await lineProcess.ReplyAsync(json.events[0].replyToken,
            //   " this is a test  ");

            bool expectedResult = false;
            //if (lineProcess.IsSingature(xLineSignature, requestBody))
            //{
            //    expectedResult = true;
            //    await lineProcess.ReplyAsync(json.events[0].replyToken,
            //      " this is a test  ");
               
            //}

            // Assert
            Assert.AreEqual(expectedResult, true);

        }

         
    }
}