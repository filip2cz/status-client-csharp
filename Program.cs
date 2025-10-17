using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace status_client_csharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string configPath = CheckConfig();
            Console.WriteLine($"Loading config found at {configPath}, loading it.");
            dynamic config = LoadConfig(configPath);

            Debug.WriteLine("Main(): Loaded config:");
            Debug.WriteLine($"config.server = {config.server}");
            Debug.WriteLine($"config.port = {config.port}");
            Debug.WriteLine($"config.user = {config.user}");
            Debug.WriteLine($"config.passwd = {config.passwd}");
        }
        static string CheckConfig()
        {
            if (File.Exists("config.json") == true)
            {
                Debug.WriteLine($"CheckConfig(): Config found at {Environment.CurrentDirectory}\\config.json");
                return $"{Environment.CurrentDirectory}\\config.json";
            }
            else if (File.Exists($"{AppContext.BaseDirectory}\\config.json") == true)
            {
                Debug.WriteLine($"CheckConfig(): Config found at {AppContext.BaseDirectory}config.json");
                return $"{AppContext.BaseDirectory}config.json";
            }
            else
            {
                Console.WriteLine("config.json not found, creating one");

                using (StreamWriter sw = File.CreateText($"{AppContext.BaseDirectory}config.json"))
                {
                    sw.WriteLine("{");
                    sw.WriteLine("  \"server\": \"example.com\",");
                    sw.WriteLine("  \"port\": 35601,");
                    sw.WriteLine("  \"user\": \"user\",");
                    sw.WriteLine("  \"passwd\": \"password\"");
                    sw.WriteLine("}");
                }

                Console.WriteLine($"config json created in: {AppContext.BaseDirectory}config.json");

                Environment.Exit(0);
                return "";
            }
        }
        static dynamic LoadConfig(string configPath)
        {
            var configJson = File.ReadAllText(configPath);
            dynamic config = JsonConvert.DeserializeObject(configJson);
            return config;
        }
    }
}
