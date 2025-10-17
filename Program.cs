using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

            while (true)
            {
                Debug.WriteLine("Creating TcpClient instance");

                TcpClient client = new TcpClient();

                Console.WriteLine($"Connecting to {config.server}:{config.port}");
                while (!client.Connected)
                {
                    try
                    {
                        client.Connect(Convert.ToString(config.server), Convert.ToInt32(config.port));
                        Console.WriteLine($"Connected to {config.server}:{config.port}");
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("SocketException: " + ex.Message);
                        Console.WriteLine("Connection refused, trying again.");
                        Thread.Sleep(1000);
                    }
                }

                Debug.WriteLine("Main(): Creating Network Stream");
                NetworkStream streamLogin = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;

                Console.WriteLine($"Authenticating with user {config.user}");

                // read data from server
                bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
                Debug.WriteLine("Server output:");
                Debug.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                // send data to server
                string login = $"{config.user}:{config.passwd}\r\n";
                byte[] loginData = Encoding.ASCII.GetBytes(login);
                streamLogin.Write(loginData, 0, loginData.Length);

                // read data from server
                bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
                Debug.WriteLine($"Server output: {Encoding.ASCII.GetString(buffer, 0, bytesRead)}");
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                Thread.Sleep(10000);
            }
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
