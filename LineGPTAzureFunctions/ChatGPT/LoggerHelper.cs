using Microsoft.Extensions.Logging;

namespace LineGPTAzureFunctions.Helper
{
    public static class LoggerHelper
    {
        public static ILogger GetLogger<T>()
        {
            ILoggerFactory loggerFactory = new LoggerFactory();      
            return loggerFactory.CreateLogger<T>();
        }
    }
}
