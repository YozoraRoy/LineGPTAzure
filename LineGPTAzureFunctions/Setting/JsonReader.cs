using ShareLibrary.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.IO;

namespace ShareLibrary.Setting
{
    public class JsonReader
    {
        public async Task<ChatSetupObject> ReadJsonData()
        {
            try
            {
                // string projectAFolderPath = @"..\ClassLibrary\Setting\"; 
                // string dataFilePath = Path.Combine(projectAFolderPath, "ChatSetup.json");

                string replacestring = @"ConsoleAppFreud\bin\Debug\net7.0";
                string tarGetString = @"ClassLibrary\Setting\ChatSetup.json";
                string jsonFilePath = Directory.GetCurrentDirectory().Replace(replacestring, tarGetString);

                string jsonContent = File.ReadAllText(jsonFilePath);
                ChatSetupObject myData = JsonConvert.DeserializeObject<ChatSetupObject>(jsonContent);

                return myData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<string> ReadJsonPathAndString()
        {
            try
            {
                // string projectAFolderPath = @"..\ClassLibrary\Setting\"; 
                // string dataFilePath = Path.Combine(projectAFolderPath, "ChatSetup.json");

                string replacestring = @"ConsoleAppFreud\bin\Debug\net7.0";
                string tarGetString = @"ClassLibrary\Setting\ChatSetup.json";
                string jsonFilePath = Directory.GetCurrentDirectory().Replace(replacestring, tarGetString);

                string jsonContent = File.ReadAllText(jsonFilePath);
                 

                string myData = $"jsonFilePath:{jsonFilePath} and jsonContent: {jsonContent}";
                return myData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }


    }
}
