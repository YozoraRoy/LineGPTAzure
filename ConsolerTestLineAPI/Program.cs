using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using Newtonsoft.Json;
using LineGPTAzureFunctions.DB;
using OpenAI_API.Chat;

namespace CosmosGettingStartedTutorial
{
    class Program
    {
        // <Main>
        public static async Task Main(string[] args)
        {
            LineMenuCustomization lineMenuCustomization = new LineMenuCustomization();

            //var result01 = await lineMenuCustomization.CreatedRichMenu();


            // var result02 = await lineMenuCustomization.UploadRichMenuImage();


            // var result03 = await lineMenuCustomization.GetAllRichMenus();


            // var result04 = await lineMenuCustomization.AssignRichMenu(true);


            // var result5 = await lineMenuCustomization.AssignRichMenu(false);

 

            Console.WriteLine("s");
        }



    }
}
