using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;

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

                while (client.Connected)
                {
                    var memory = GetMemoryInfo();

                    string data = "update {\"online6\": " + CheckIPv6Support() + ",  \"uptime\": " + GetUptime() + ", \"load\": -1.0, \"memory_total\": " + memory.ramTotal + ", \"memory_used\": " + (memory.ramTotal - memory.ramFree) + ", \"swap_total\": " + memory.swapTotal + ", \"swap_used\": " + (memory.swapTotal - memory.swapFree) + ", \"hdd_total\": " + "1" + ", \"hdd_used\": " + "0" + ", \"cpu\": " + "0" + ".0, \"network_rx\": " + "0" + ", \"network_tx\": " + "0" + " }\r\n";
                    Debug.WriteLine($"Main(): data = {data}");
                    byte[] dataSend = Encoding.ASCII.GetBytes(data);
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(dataSend, 0, dataSend.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Connection refused: " + ex.Message);
                    }
                    Thread.Sleep(3000);
                }

                Console.WriteLine("Client disconnected, connecting again");
                Debug.WriteLine("Main(): Client disconnected, connecting again");
                Thread.Sleep(1000);
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
        static string CheckIPv6Support()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("ipv6.google.com");
                Debug.WriteLine($"CheckIPv6Support(): {reply.Status == IPStatus.Success}");
                return Convert.ToString(reply.Status == IPStatus.Success);
            }
            catch
            {
                Debug.WriteLine($"CheckIPv6Support(): false");
                return "false";
            }
        }
        static string GetUptime()
        {
            var uptimeMilliseconds = System.Environment.TickCount;
            var uptimeSeconds = (long)(uptimeMilliseconds / 1000);
            Debug.WriteLine($"GetUptime(): uptimeSeconds = {uptimeSeconds}");
            return uptimeSeconds.ToString();
        }
        static dynamic GetMemoryInfo()
        {
            // RAM + Swap
            // https://ourcodeworld.com/articles/read/879/how-to-retrieve-the-ram-amount-available-on-the-system-in-winforms-with-c-sharp
            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            ManagementObjectCollection results = searcher.Get();

            List<int> memoryValues = new List<int>(); // inicialization of list for results

            foreach (ManagementObject result in results)
            {
                int totalVisibleMemory = Convert.ToInt32(result["TotalVisibleMemorySize"]);
                int freePhysicalMemory = Convert.ToInt32(result["FreePhysicalMemory"]);
                int totalVirtualMemory = Convert.ToInt32(result["TotalVirtualMemorySize"]);
                int freeVirtualMemory = Convert.ToInt32(result["FreeVirtualMemory"]);

                memoryValues.Add(totalVisibleMemory);
                memoryValues.Add(freePhysicalMemory);
                memoryValues.Add(totalVirtualMemory);
                memoryValues.Add(freeVirtualMemory);
            }

            var memory = new
            {
                ramTotal = memoryValues[0],
                ramFree = memoryValues[1],
                swapTotal = memoryValues[2],
                swapFree = memoryValues[3]
            };

            return memory;
        }
    }
}
