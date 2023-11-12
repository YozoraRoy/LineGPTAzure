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
        protected Database database; // The database we will create
        protected Microsoft.Azure.Cosmos.Container container;// The container we will create.
                                                           // The name of the database and container we will create

        //// The Azure Cosmos DB endpoint for running this sample.
        public string EndpointUri = string.Empty;

        //// The primary key for the Azure Cosmos account.
        public string PrimaryKey = string.Empty;
        public string databaseId = string.Empty;
        public string containerId = string.Empty;
        public string userSettingContainerId = string.Empty;
        public string UserSettingDefaultId = string.Empty;
        public string promptSetup = string.Empty;
        ILogger _log;
        public static bool isInFiveMintue = true;
        public static bool isNewConversation = false;

        public CosmosProcess()
        { }

        public CosmosProcess(ILogger log) {
            _log = log;
            GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);

        }

        /// <summary>
        /// Proccess the gpt message 
        /// 1. Within 5 minutes
        /// 2. set up
        /// </summary>
        public async Task<List<ChatMessage>> ChatGPTMessagePorcAsync(string userId, string userDisplayName, string usermeeage)
        {

            GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);

            List<ChatMessage> chatMessageList = new List<ChatMessage>();

            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            
            await this.CreateDatabaseAsync();
            _log.LogInformation($"CreateDatabaseAsync OK");
            await this.CreateContainerAsync();
            _log.LogInformation($"CreateContainerAsync OK");

            // find data
            var cosmosdbresult = await this.QueryChatMsgAsync(userId);

            if (cosmosdbresult.Count == 0)
            {  // 1. new conversation 
                isNewConversation = true;
                _log.LogInformation($"New conversation: / {cosmosdbresult.Count} / isInFiveMintue:{isInFiveMintue}");
                MessageMapping messageMapping = new MessageMapping(_log);
                _log.LogInformation($"New messageMapping");
                chatMessageList = await messageMapping.NewConversationMessage(userId, userDisplayName, usermeeage, chatMessageList);
                _log.LogInformation($"New conversation Msg: / {JsonConvert.SerializeObject(chatMessageList)} ");
            }
            else
            {// Continue conversation
                var result = cosmosdbresult.FirstOrDefault();
                var finalDataTime = DateTime.Parse(result.finalDataTime);

                // chekc time more then 5 min
                TimeSpan difference = DateTime.Now - finalDataTime;
                bool isOver5Minutes = difference.TotalMinutes > 5;
                _log.LogInformation($"Chekc time more then 5 min: / difference.TotalMinutes:{difference.TotalMinutes} / Now {DateTime.Now} Fina {finalDataTime} / isOver5Minutes: {isOver5Minutes} / isInFiveMintue:{isInFiveMintue}");

                if (isOver5Minutes)
                {
                    isInFiveMintue = false;
                    isNewConversation = true;
                    _log.LogInformation($"Continue conversation - more then 5 min: / result.chatMessage.Count : {result.chatMessage.Count} / isInFiveMintue:{isInFiveMintue} / isNewConversation:{isNewConversation}");
                    // .delete all record in CosmosDB created a new conversion
                    await DeleteItemAsync(userId);                   
                    MessageMapping messageMapping = new MessageMapping(_log);
                    chatMessageList = await messageMapping.NewConversationMessage(userId, userDisplayName, usermeeage, chatMessageList); ;
                }
                else
                {
                    isInFiveMintue = true;
                    isNewConversation = false;
                    // put message to the cosmosdb chatMessage.
                    result.chatMessage.Add(new ChatMessage(ChatMessageRole.User, usermeeage));
                    chatMessageList = result.chatMessage;
                    _log.LogInformation($"Continue conversation - not more then 5 min: /  result.chatMessage.Count : {result.chatMessage.Count}  / isInFiveMintue:{isInFiveMintue} / isNewConversation:{isNewConversation}");                     
                }
            }

            return chatMessageList;
        }

        public async Task FinalMessageDataProcess(List<ChatMessage> chatMessageList, string userId , string userDisplayName)
        {        
            if (isNewConversation)
            {
                _log.LogInformation($"Start-isNewConversation:{isNewConversation} - StoreChatMsgToContainerAsync");
                await StoreChatMsgToContainerAsync(chatMessageList, userId, userDisplayName, isInFiveMintue);
                _log.LogInformation($"End-isNewConversation:{isNewConversation} - StoreChatMsgToContainerAsync");                
            }
            else if (isInFiveMintue)
            {
                _log.LogInformation($"Start-isInFiveMintue:{isInFiveMintue} -ReplaceMsgItemAsync");
                await ReplaceMsgItemAsync(chatMessageList, userId, isInFiveMintue);
                _log.LogInformation($"Ens-isInFiveMintue:{isInFiveMintue}-ReplaceMsgItemAsync");
            }            
        }

        protected static void GetLocalSetting(out string EndpointUri, out string PrimaryKey, out string containerId, out string databaseId, out string userSettingContainerId)
        {
            KeyValueSetting keyValueSetting = new KeyValueSetting();
            EndpointUri = keyValueSetting.CosmosEndpointUri; 
            PrimaryKey = keyValueSetting.CosmosPrimaryKey; 
            containerId = keyValueSetting.chatContainerId;
            userSettingContainerId = keyValueSetting.UserSettingContainerId;
            databaseId = keyValueSetting.databaseId;
        }

        /// <summary>
        /// Run a query 
        /// </summary>
        protected async Task<List<CosmosDBMessageRecorder>> QueryChatMsgAsync(string userId)
        {
           
            // var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}' and c.finalDataTime >= '{f5}'";
            var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}'";

            // _log.LogInformation($"QueryChatMsgAsync: / {sqlQueryText}");

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
                }
            }

            return recorders;
        }

        /// <summary>
        /// Add Family items to the container
        /// </summary>
        private async Task StoreChatMsgToContainerAsync(List<ChatMessage> chatMessages, string userId, string userDisplayName, bool isFirstFiveMinute)
        {
            List<ChatMessage> userMsg = new List<ChatMessage>();
            userMsg = chatMessages;

            CosmosDBMessageRecorder cosmosDBMessageRecorder = new CosmosDBMessageRecorder
            {
                Id = userId,
                PartitionKey = "JapanPartition",
                finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                chatMessage = userMsg,
                userName = userDisplayName,
                isInFiveMintue = isFirstFiveMinute,
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
        public async Task DeleteItemAsync(string userId)
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
                _log.LogInformation($"Cosmos Delete Userid : /{userId} ex: / {ex.Message} / {ex}");
                throw;
            }

            // Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, userId);
        }

        /// <summary>
        /// Delete an item in the container
        /// </summary>
        public async Task DeleteItemAsyncWithInstance(string userId)
        {
            GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);

            var partitionKeyValue = "JapanPartition";
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();

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
                _log.LogInformation($"Cosmos Delete Userid : /{userId} ex: / {ex.Message} / {ex}");
                throw;
            }

            // Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKeyValue, userId);
        }

        /// <summary>
        /// Replace an item in the container
        /// </summary>
        private async Task ReplaceMsgItemAsync(List<ChatMessage> chatMessages, string userId, bool isFirstFiveMinute)
        {
            ItemResponse<CosmosDBMessageRecorder> msgResponse = await this.container.ReadItemAsync<CosmosDBMessageRecorder>(userId, new PartitionKey("JapanPartition"));
            var itemBody = msgResponse.Resource;
             
            itemBody.chatMessage = chatMessages;
            itemBody.finalDataTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            itemBody.isInFiveMintue = isFirstFiveMinute;
            await this.container.ReplaceItemAsync<CosmosDBMessageRecorder>(itemBody, itemBody.Id, new PartitionKey(itemBody.PartitionKey));
            // Console.WriteLine("Updated Family [{0},{1}].\n \tBody is now: {2}\n", itemBody.userName, itemBody.finalDataTime, msgResponse.Resource);
        }


        public async Task<string> QueryUserPromptWithDefaultAsync(string userId)
        {
            string result = await QueryUserPromptAsync(userId);

            if (string.IsNullOrWhiteSpace(result))
            {
                result = await QueryUserPromptAsync(string.Empty);
            }

            return  result;
        }

        /// <summary>
        /// Run a Prompt query  
        /// </summary>
        private async Task<string> QueryUserPromptAsync(string userId)
        {
            if (this.container == null)
            {
                GetLocalSetting(out EndpointUri, out PrimaryKey, out databaseId, out containerId, out userSettingContainerId);
                // Create a new instance of the Cosmos Client
                CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBUserSettingTest" });
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                Microsoft.Azure.Cosmos.Container container = await database.CreateContainerIfNotExistsAsync(userSettingContainerId, "/partitionKey");
                this.container = container;
            }             

            if (string.IsNullOrEmpty(userId))
            {
                KeyValueSetting keyValueSetting = new KeyValueSetting();
                userId = keyValueSetting.UserSettingDefaultId;
            }
             
            string result = string.Empty;

            var sqlQueryText = $"SELECT * FROM c WHERE c.partitionKey = 'JapanPartition' and c.id = '{userId}'";

            if (this.container == null)
            {
                this.container = await this.database.CreateContainerIfNotExistsAsync(userSettingContainerId, "/partitionKey");
            }

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<CosmosDBUserSetting> queryResultSetIterator = container.GetItemQueryIterator<CosmosDBUserSetting>(queryDefinition);
 
            List<CosmosDBUserSetting> recorders = new List<CosmosDBUserSetting>();

            if (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<CosmosDBUserSetting> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (CosmosDBUserSetting cosmosDBMessageRecorder in currentResultSet)
                {
                    result = cosmosDBMessageRecorder.gptPromptsSetting;
                }
                 
            }
              
            return result;
        }

         
        #region "Basic function"

   
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        protected async Task CreateDatabaseAsync()
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
        protected async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
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
