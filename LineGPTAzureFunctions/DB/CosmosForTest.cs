using CosmosGettingStartedTutorial;
using LineGPTAzureFunctions.Helper;
using Microsoft.Azure.Cosmos;
using Microsoft.CognitiveServices.Speech.Transcription;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.DB
{
    public class CosmosForTest: CosmosProcess
    {
        private CosmosClient cosmosClient; // The Cosmos client instance
        

        public CosmosForTest() { 
        
        
        }


        public async Task ExecUserSettingDataTest()
        {
            try
            {
                Console.WriteLine("Beginning operations...");

                await ExecUserSettingDemoAsync();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task ExecUserSettingDemoAsync()
        {

            GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);

            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBUserSettingTest" });
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            this.container = await this.database.CreateContainerIfNotExistsAsync(userSettingContainerId, "/partitionKey");
           
            // await this.ScaleContainerAsync();
            // await this.AddUserSettingToContainerAsync();
            // await this.AddItemsToContainerAsync();
            // await this.QueryUserMsgAsync("U6fa80bda7dcb81a8a51d926ff36d832b");
              
            string userId = "U6fa80bda7dcb81a8a51d926ff36d832b";

            string userDisplayName = string.Empty;
            string userName = string.Empty;
            string usermeeage = string.Empty;
            MessageMapping messageMapping = new MessageMapping();
            List<ChatMessage> chatMessageList = new List<ChatMessage>();
            var chatMessageListResult = await messageMapping.NewConversationMessageWithoutLog(userId, userDisplayName, usermeeage, chatMessageList);

           
            await this.QueryUserPromptWithDefaultAsync(userId);

            // await this.QueryUserMsgAsyncAndReplace("00001");
            // await DeleteItemAsync("00001");
            // await this.QueryItemsAsync();
            // await this.ReplaceFamilyItemAsync();
            // await this.DeleteFamilyItemAsync();
            // await this.DeleteDatabaseAndCleanupAsync();
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddUserSettingToContainerAsync()
        {
            // Create a family object for the Andersen family
            CosmosDBUserSetting cosmosDBMessageRecorder = new CosmosDBUserSetting
            {
                Id = "00001",
                PartitionKey = $"JapanPartition",
                dataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                userName = "Roy",
                gptPromptsSetting = "this is a test Prompt Message",
            };

            try
            {
                ItemResponse<CosmosDBUserSetting> cosmosDBMessageResponse = await this.container.ReadItemAsync<CosmosDBUserSetting>(cosmosDBMessageRecorder.Id, new PartitionKey(cosmosDBMessageRecorder.PartitionKey));
                Console.WriteLine("Item in database with id: {0} already exists\n", cosmosDBMessageResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                  ItemResponse<CosmosDBUserSetting> cosmosDBMessageResponse = await this.container.CreateItemAsync<CosmosDBUserSetting>(cosmosDBMessageRecorder, new PartitionKey(cosmosDBMessageRecorder.PartitionKey));

                  Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", cosmosDBMessageResponse.Resource.Id, cosmosDBMessageResponse.RequestCharge);
            }
        }


        /// <summary>
        /// Run a query 
        /// </summary>
        protected async Task<List<CosmosDBUserSetting>> QueryDBUserPromptAsync(string userId)
        {
            // var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}' and c.finalDataTime >= '{f5}'";
            var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}'";

            // _log.LogInformation($"QueryChatMsgAsync: / {sqlQueryText}");

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CosmosDBUserSetting> queryResultSetIterator = this.container.GetItemQueryIterator<CosmosDBUserSetting>(queryDefinition);

            List<CosmosDBUserSetting> recorders = new List<CosmosDBUserSetting>();

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CosmosDBUserSetting> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CosmosDBUserSetting cosmosDBMessageRecorder in currentResultSet)
                {
                    recorders.Add(cosmosDBMessageRecorder);
                }
            }

            return recorders;
        }


        #region "UserChatDataTest"

        public async Task ExecUserDataTest()
        {
            try
            {
                Console.WriteLine("Beginning operations...");

                await ExecUserSettingDemoAsync();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
         
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task ExecUserDataDemoAsync()
        {

            GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);


            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            // await this.ScaleContainerAsync();
            // await this.AddUserMsgToContainerAsync();
            // await this.AddItemsToContainerAsync();
            // await this.QueryUserMsgAsync("U6fa80bda7dcb81a8a51d926ff36d832b");

            // await this.QueryUserPromptAsync("U6fa80bda7dcb81a8a51d926ff36d832b");


            // await this.QueryUserMsgAsyncAndReplace("00001");

            // await DeleteItemAsync("00001");
            // await this.QueryItemsAsync();
            //await this.ReplaceFamilyItemAsync();
            //await this.DeleteFamilyItemAsync();
            //await this.DeleteDatabaseAndCleanupAsync();
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddUserMsgToContainerAsync()
        {
            List<ChatMessage> userMsg = new List<ChatMessage>();
            ChatMessage row = new ChatMessage();
            userMsg.Add(new ChatMessage(ChatMessageRole.System, promptSetup));
            userMsg.Add(new ChatMessage(ChatMessageRole.Assistant, "Hi，Roy!!"));

            // Create a family object for the Andersen family
            CosmosDBMessageRecorder cosmosDBMessageRecorder = new CosmosDBMessageRecorder
            {
                Id = "00001",
                PartitionKey = $"JapanPartition",
                finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                userName = "Roy"
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<CosmosDBMessageRecorder> cosmosDBMessageResponse = await this.container.ReadItemAsync<CosmosDBMessageRecorder>(cosmosDBMessageRecorder.Id, new PartitionKey(cosmosDBMessageRecorder.PartitionKey));
                Console.WriteLine("Item in database with id: {0} already exists\n", cosmosDBMessageResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<CosmosDBMessageRecorder> cosmosDBMessageResponse = await this.container.CreateItemAsync<CosmosDBMessageRecorder>(cosmosDBMessageRecorder, new PartitionKey(cosmosDBMessageRecorder.PartitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", cosmosDBMessageResponse.Resource.Id, cosmosDBMessageResponse.RequestCharge);
            }

        }

        #endregion

    }
}
