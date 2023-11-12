using StackExchange.Redis;

namespace ConsoleTestRedis
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Connect to Redis
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("AIRds.redis.cache.windows.net:6380,password=7nZZDz2YPLrcAmJRfa70eiLdMpvJHF9GyAzCaBiXDnI=,ssl=True,abortConnect=False");

            // Get a database
            IDatabase db = redis.GetDatabase();

            // Set a value and Max lenth is 1024 bytes 
            db.StringSet("key", "value");

            // Get a value
            string value = db.StringGet("key");

            Console.WriteLine(value);

           // Console.WriteLine("Hello, World!");
        }
    }
}