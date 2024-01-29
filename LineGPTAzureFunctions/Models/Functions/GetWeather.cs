using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LineFunctionApp.Models.Functions
{
    public class GetWeatherFunction
    {
        static public string Name = "get_current_weather";

        // Return the function metadata
        static public FunctionDefinition GetFunctionDefinition()
        {
            return new FunctionDefinition()
            {
                Name = Name,
                Description = "Get the current weather in a given location",
                Parameters = BinaryData.FromObjectAsJson(
                new
                {
                    Type = "object",
                    Properties = new
                    {
                        Location = new
                        {
                            Type = "string",
                            Description = "The city and state, e.g. San Francisco, CA",
                        },
                        Unit = new
                        {
                            Type = "string",
                            Enum = new[] { "Celsius", "Fahrenheit" },
                        }
                    },
                    Required = new[] { "location" },
                },
                new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            };
        }

        // The function implementation. It always returns 31 for now.
        static public Weather GetWeather(string location, string unit)
        {
            return new Weather() { Temperature = 31, Unit = unit };
        }
    }

    // Argument for the function
    public class WeatherInput
    {
        public string Location { get; set; } = string.Empty;
        public string Unit { get; set; } = "Celsius";
    }

    // Return type
    public class Weather
    {
        public int Temperature { get; set; }
        public string Unit { get; set; } = "Celsius";
    }
}
