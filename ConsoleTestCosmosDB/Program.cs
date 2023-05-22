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
            CosmosProcess cosmosProcess = new CosmosProcess();
            await cosmosProcess.ExecDataTest();
        }
          
  
         
    }
}
