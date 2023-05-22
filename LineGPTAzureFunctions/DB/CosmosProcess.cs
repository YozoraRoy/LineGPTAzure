using Azure;
using CosmosGettingStartedTutorial;
using LineGPTAzureFunctions.Helper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.DB
{
    public class CosmosProcess
    {
        private CosmosClient cosmosClient; // The Cosmos client instance
        private Database database; // The database we will create
        private Microsoft.Azure.Cosmos.Container container;// The container we will create.
                                                           // The name of the database and container we will create

        //// The Azure Cosmos DB endpoint for running this sample.
        public static string EndpointUri = string.Empty;

        //// The primary key for the Azure Cosmos account.
        public static string PrimaryKey = string.Empty;
        public string databaseId = string.Empty;
        public string containerId = string.Empty;
        public string systemSetup = string.Empty;
        ILogger _log;

        public CosmosProcess()
        {            }

        public CosmosProcess(ILogger log) {
            _log = log;
        }

        /// <summary>
        /// Proccess the gpt message 
        /// 1. Within 5 minutes
        /// 2. set up
        /// </summary>
        public async Task<List<ChatMessage>> ChatGPTMessagePorcAsync(string userId, string userDisplayName, string usermeeage)
        {

            GetLocalSetting(out systemSetup, out EndpointUri, out PrimaryKey, out databaseId, out containerId);

            List<ChatMessage> chatMessageList = new List<ChatMessage>();

            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            _log.LogInformation($"EndpointUri: {EndpointUri}",$"PrimaryKey:{PrimaryKey}",$"containerId:{containerId}",$"containerId{containerId}");

            await this.CreateDatabaseAsync();
            _log.LogInformation($"CreateDatabaseAsync OK");
            await this.CreateContainerAsync();
            _log.LogInformation($"CreateContainerAsync OK");

            // find data
            var cosmosdbresult = await this.QueryChatMsgAsync(userId);
          
            if (cosmosdbresult.Count == 0)
            {  // 1. new conversion 
                _log.LogInformation($"New conversion: / {cosmosdbresult.Count}");
                await NewConversionStore(userId, userDisplayName, usermeeage, chatMessageList);
            }
            else
            {// Continue conversion
              
                var result = cosmosdbresult.FirstOrDefault();                
                var finalDataTime = DateTime.Parse(result.finalDataTime);

                // chekc time more then 5 min
                TimeSpan difference = DateTime.Now - finalDataTime;
                bool isDifferenceWithin5Minutes = difference.TotalMinutes > 5;

                _log.LogInformation($"Chekc time more then 5 min: / {finalDataTime} / {DateTime.Now} / isDifferenceWithin5Minutes");

                if (isDifferenceWithin5Minutes)
                {
                    _log.LogInformation($"Continue conversion - more then 5 min: / {result.chatMessage.Count} : {result.chatMessage}");

                    // 1.delete all record
                    await DeleteItemAsync(userId);

                    // 2.store new data to cosmosdb
                    await NewConversionStore(userId, userDisplayName, usermeeage, chatMessageList);
                }
                else
                {
                    // put message to the cosmosdb chatMessage.
                    result.chatMessage.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
                    
                    chatMessageList = result.chatMessage;
                    _log.LogInformation($"Continue conversion - not more then 5 min: / {result.chatMessage.Count} : {result.chatMessage}");
                    
                    // replace new data to cosmosdb
                    await ReplaceMsgItemAsync(chatMessageList, userId);
                }
            }

            return chatMessageList;
        }

        private async Task NewConversionStore(string userId, string userDisplayName, string usermeeage, List<ChatMessage> chatMessageList)
        {
            string mergemsg = string.Format("{0},{1}", systemSetup, userDisplayName);

            chatMessageList.Add(new ChatMessage(ChatMessageRole.System, mergemsg)); // Set CharGpt Role
            chatMessageList.Add(new ChatMessage(ChatMessageRole.Assistant, $"Hello  ! {userDisplayName}"));
            chatMessageList.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
            // 2.store new data to cosmosdb
            await StoreChatMsgToContainerAsync(chatMessageList, userId, userDisplayName);
        }

        private static void GetLocalSetting(out string systemSetup, out string EndpointUri, out string PrimaryKey, out string containerId, out string databaseId)
        {
            var config = new ConfigurationBuilder()
                             .SetBasePath(Environment.CurrentDirectory)
                             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                             .Build();
            KeyValueSetting keyValueSetting = new KeyValueSetting();
            systemSetup = keyValueSetting.systemSetup; 
            EndpointUri = keyValueSetting.CosmosEndpointUri; 
            PrimaryKey = keyValueSetting.CosmosPrimaryKey; 
            containerId = keyValueSetting.containerId;
            databaseId = keyValueSetting.databaseId; 
        }

        /// <summary>
        /// Run a query 
        /// </summary>
        private async Task<List<CosmosDBMessageRecorder>> QueryChatMsgAsync(string userId)
        {
            string f5 = GetFivemMnDatatime();
            // var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}' and c.finalDataTime >= '{f5}'";
            var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}'";

            _log.LogInformation($"QueryChatMsgAsync: / {sqlQueryText}");

            // Console.WriteLine("Running query: {0}\n", sqlQueryText);
            
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CosmosDBMessageRecorder> queryResultSetIterator = this.container.GetItemQueryIterator<CosmosDBMessageRecorder>(queryDefinition);

            List<CosmosDBMessageRecorder> recorders = new List<CosmosDBMessageRecorder>();

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CosmosDBMessageRecorder> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CosmosDBMessageRecorder cosmosDBMessageRecorder in currentResultSet)
                {
                    recorders.Add(cosmosDBMessageRecorder);
                }
            }

            return recorders;
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task StoreChatMsgToContainerAsync(List<ChatMessage> chatMessages, string userId, string userDisplayName)
        {
            List<ChatMessage> userMsg = new List<ChatMessage>();
            userMsg = chatMessages;

            CosmosDBMessageRecorder cosmosDBMessageRecorder = new CosmosDBMessageRecorder
            {
                Id = userId,
                PartitionKey = "JapanPartition",
                finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                chatMessage = userMsg,
                userName = userDisplayName
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

        /// <summary>
        /// Delete an item in the container
        /// </summary>
        private async Task DeleteItemAsync(string userId)
        {
            var partitionKeyValue = "JapanPartition";

            try
            {
                _log.LogInformation($"Cosmos Delete: / {userId}");
                // Delete an item. Note we must provide the partition key value and id of the item to delete
                ItemResponse<CosmosDBMessageRecorder> response = await this.container.DeleteItemAsync<CosmosDBMessageRecorder>(userId, new PartitionKey(partitionKeyValue));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                // Console.WriteLine("Delete item in database with id: {0} Operation consumed {1} RUs.\n", userId, response.RequestCharge);
            }
             catch (Exception ex)
            {
                _log.LogInformation($"Cosmos Delete ex: / {ex.Message} / {ex}");
                // Other
            }

            // Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, userId);
        }

        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private async Task ReplaceMsgItemAsync(List<ChatMessage> chatMessages, string userId)
        {
            ItemResponse<CosmosDBMessageRecorder> msgResponse = await this.container.ReadItemAsync<CosmosDBMessageRecorder>(userId, new PartitionKey("JapanPartition"));
            var itemBody = msgResponse.Resource;
             
            itemBody.chatMessage = chatMessages;
            itemBody.finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await this.container.ReplaceItemAsync<CosmosDBMessageRecorder>(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));
            // Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.userName, itemBody.finalDataTime, msgResponse.Resource);
        }

        private static string GetFivemMnDatatime()
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(-5); // 5 分鐘的 TimeSpan
            DateTime dateTime = DateTime.Now; // 現在的日期時間
            DateTime newDateTime = dateTime.Add(timeSpan); // 加上 TimeSpan 後的日期時間

            //DateTime s1 = DateTime.Now;
            //DateTime s2 = DateTime.Now.AddMinutes(-5);
            //var f5 = (s1 -s2).ToString("yyyy-MM-dd HH:mm:ss");

            return newDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #region "Basic function"

        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task GetStartedDemoAsync()
        {

            GetLocalSetting(out systemSetup, out EndpointUri, out PrimaryKey, out databaseId, out containerId);


            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            // await this.ScaleContainerAsync();
            await this.AddUserMsgToContainerAsync();
            // await this.AddItemsToContainerAsync();
            await this.QueryUserMsgAsync("00001");

            await this.QueryUserMsgAsyncAndReplace("00001");

            await DeleteItemAsync("00001");
            // await this.QueryItemsAsync();
            //await this.ReplaceFamilyItemAsync();
            //await this.DeleteFamilyItemAsync();
            //await this.DeleteDatabaseAndCleanupAsync();
        }

        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task AddUserMsgToContainerAsync()
        {
            List<ChatMessage> userMsg = new List<ChatMessage>();
            ChatMessage row = new ChatMessage();
            userMsg.Add(new ChatMessage(ChatMessageRole.System, systemSetup));
            userMsg.Add(new ChatMessage(ChatMessageRole.Assistant, "妳好，羅伊!!"));

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

        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        /// </summary>
        private async Task QueryUserMsgAsync(string userId)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CosmosDBMessageRecorder> queryResultSetIterator = this.container.GetItemQueryIterator<CosmosDBMessageRecorder>(queryDefinition);

            // List<string> recorders = new List<string>();

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CosmosDBMessageRecorder> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CosmosDBMessageRecorder cosmosDBMessageRecorder in currentResultSet)
                {
                    // recorders.Add(JsonConvert.SerializeObject(cosmosDBMessageRecorder));
                    Console.WriteLine("\tRead {0}\n", JsonConvert.SerializeObject(cosmosDBMessageRecorder));
                }
            }
        }

        /// <summary>
        /// QueryReplace an item in the container
        /// </summary>
        private async Task QueryUserMsgAsyncAndReplace(string userId)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.id = '{userId}'";
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CosmosDBMessageRecorder> queryResultSetIterator = this.container.GetItemQueryIterator<CosmosDBMessageRecorder>(queryDefinition);

            List<CosmosDBMessageRecorder> recorders = new List<CosmosDBMessageRecorder>();
            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CosmosDBMessageRecorder> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CosmosDBMessageRecorder cosmosDBMessageRecorder in currentResultSet)
                {
                    recorders.Add(cosmosDBMessageRecorder);
                    // Console.WriteLine("\tRead {0}\n", JsonConvert.SerializeObject(cosmosDBMessageRecorder));
                }
            }

            var replaceItem = recorders.FirstOrDefault();

            ItemResponse<CosmosDBMessageRecorder> msgResponse = await this.container.ReadItemAsync<CosmosDBMessageRecorder>(replaceItem.Id, new PartitionKey(replaceItem.PartitionKey));
            var itemBody = msgResponse.Resource;

            // update registration status from false to true
            itemBody.userName = "Tom";
            // update grade of child
            itemBody.finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // replace the item with the updated content
            msgResponse = await this.container.ReplaceItemAsync<CosmosDBMessageRecorder>(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));
            // Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.userName, itemBody.finalDataTime, msgResponse.Resource);
        }

        #region "Example"

        public async Task ExecDataTest()
        {
            try
            {
                Console.WriteLine("Beginning operations...\n");

                await GetStartedDemoAsync();
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
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        /// <returns></returns>
        private async Task ScaleContainerAsync()
        {
            // Read the current throughput
            try
            {
                int? throughput = await this.container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await this.container.ReplaceThroughputAsync(newThroughput);
                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Cannot read container throuthput.");
                Console.WriteLine(cosmosException.ResponseBody);
            }
        }

        /// <summary>
        /// Delete the database and dispose of the Cosmos Client instance
        /// </summary>
        private async Task DeleteDatabaseAndCleanupAsync()
        {
            DatabaseResponse databaseResourceResponse = await this.database.DeleteAsync();
            // Also valid: await this.cosmosClient.Databases["FamilyDatabase"].DeleteAsync();

            Console.WriteLine("Deleted Database: {0}\n", this.databaseId);

            //Dispose of CosmosClient
            this.cosmosClient.Dispose();
        }

        #endregion

        #endregion



    }
}
