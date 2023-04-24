using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Management;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;

Console.WriteLine("Status client C# v1.0");
Console.WriteLine("by Filip Komárek");
Console.WriteLine("Github: https://github.com/filip2cz/status-client-csharp");
Console.WriteLine("Gitea mirror: https://git.envs.net/filip2cz/status-client-csharp");

bool auth = false;
int processorCount = Environment.ProcessorCount;
string ipv6;
//int pingTries = 0;

if (File.Exists("config.json") == false)
{
    Console.WriteLine("config.json not found, creating one");

    using (StreamWriter sw = File.CreateText("config.json"))
    {
        sw.WriteLine("{");
        sw.WriteLine("  \"server\": \"example.com\",");
        sw.WriteLine("  \"port\": 35601,");
        sw.WriteLine("  \"user\": \"user\",");
        sw.WriteLine("  \"passwd\": \"password\",");
        sw.WriteLine("  \"debug\": false");
        sw.WriteLine("}");
    }

    Environment.Exit(0);
}

// load config file
var configJson = File.ReadAllText("config.json");
dynamic config = JsonConvert.DeserializeObject(configJson);

string server = config.server;
int port = config.port;
string user = config.user;
string passwd = config.passwd;
bool debug = config.debug;
//bool pingTest = config.pingtest;

// test if server is avaible with ping
/*
if (pingTest)
{
    Ping ping = new Ping();
    PingReply pingReply = ping.Send(server);
    while (pingReply.Status != IPStatus.Success)
    {
        Console.WriteLine(pingTest);
        Console.WriteLine("Ping test failed, trying again");
        pingTries++;
        Thread.Sleep(1000);
        pingReply = ping.Send(server);
        if (pingTries > 10)
        {
            Console.WriteLine("Ping failed 10 times, exiting...");
            Environment.Exit(0);
        }
    }
    if (debug)
    {
        Console.WriteLine("Ping test was successfull");
    }
    pingTries = 0;
}
else
{
    if (debug)
    {
        Console.WriteLine("Skipping ping test, because config.json said");
    }
}
*/

while (true)
{
    if (debug)
    {
        Console.WriteLine("Creating TcpClient instance");
    }

    // create TCP Client instance
    TcpClient client = new TcpClient();

    if (debug)
    {
        Console.WriteLine("Connecting to server...");
    }

    while (!client.Connected)
    {
        try
        {
            client.Connect(server, port);
            auth = true;
        }
        catch (SocketException ex)
        {
            if (debug)
            {
                Console.WriteLine("SocketException: " + ex.Message);
                Console.WriteLine("Connection refused, trying again.");
            }
            auth = false;
        }
    }

    Console.WriteLine("Connected");

    // get stream for data sending
    NetworkStream streamLogin = client.GetStream();

    byte[] buffer = new byte[1024];
    int bytesRead = 0;

    if (auth)
    {
        // read data from server
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        // autentization
        string login = $"{config.user}:{config.passwd}\r\n";
        byte[] loginData = Encoding.ASCII.GetBytes(login);
        if (debug)
        {
            Console.WriteLine($"sending: {login}");
        }
        streamLogin.Write(loginData, 0, loginData.Length);

        // read data from server
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        // read data from server
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        auth = false;
    }

    // sending
    while (client.Connected)
    {
        // network rx and tx
        // https://stackoverflow.com/questions/2081827/c-sharp-get-system-network-usage
        NetworkInterface[] interfaces
            = NetworkInterface.GetAllNetworkInterfaces();

        int network_rx = 0;
        int network_tx = 0;

        foreach (NetworkInterface ni in interfaces)
        {
            network_rx = (int)ni.GetIPv4Statistics().BytesReceived;
            network_tx = (int)ni.GetIPv4Statistics().BytesSent;
        }

        Console.WriteLine(network_rx);
        Console.WriteLine(network_tx);
        Console.WriteLine();

        // Uptime
        var uptimeMilliseconds = System.Environment.TickCount64;
        var uptimeSeconds = (long)(uptimeMilliseconds / 1000);

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

        int memoryTotal = memoryValues[0];
        int memoryFree = memoryValues[1];
        int memoryUsed = memoryTotal - memoryFree;

        int swapTotal = memoryValues[2];
        int swapFree = memoryValues[3];
        int swapUsed = swapTotal - swapFree;

        // CPU
        int cpuUsage = (int)GetCpuUsage(2000); // This method contains sleep so that it is not sent every like 2 milliseconds. If you edit this, be sure to uncomment the other sleep below

        // HDD
        DriveInfo driveC = new DriveInfo("C");
        long totalSizeInBytes = driveC.TotalSize;
        long freeSpaceInBytes = driveC.TotalFreeSpace;

        int totalSize = (int)(totalSizeInBytes / 1024 / 1024);
        long usedSpace = totalSizeInBytes - freeSpaceInBytes;

        // IPv6
        if (CheckIPv6Support())
        {
            ipv6 = "true";
        }
        else
        {
            ipv6 = "false";
        }

        // Network
        //var monitor = new NetworkTrafficMonitor();
        //monitor.StartMonitoring();

        //Console.WriteLine(monitor.BytesSent);
        //Console.WriteLine(monitor.BytesReceived);
        //Console.WriteLine(  );

        // sending
        string data = "update {\"online6\": " + ipv6 + ",  \"uptime\": " + uptimeSeconds.ToString() + ", \"load\": -1.0, \"memory_total\": " + memoryTotal + ", \"memory_used\": " + memoryUsed + ", \"swap_total\": " + swapTotal + ", \"swap_used\": " + swapUsed + ", \"hdd_total\": " + totalSize + ", \"hdd_used\": " + usedSpace / 1024 / 1024 + ", \"cpu\": " + cpuUsage + ".0, \"network_rx\": " + network_rx / 1024 + ", \"network_tx\": " + network_tx / 1024 + " }\r\n";
        byte[] dataSend = Encoding.ASCII.GetBytes(data);

        try
        {
            NetworkStream stream = client.GetStream();
            stream.Write(dataSend, 0, dataSend.Length);
        }
        catch (Exception ex)
        {
            if (debug)
            {
                Console.WriteLine("Connection refused: " + ex.Message);
            }
        }
        if (debug)
        {
            Console.WriteLine(data);
        }
        //Thread.Sleep(3000);
    }

    // disconnect
    client.Close();
    client.Dispose();
    Console.WriteLine("Disconnected");
}
static bool CheckIPv6Support()
{
    try
    {
        Ping ping = new Ping();
        PingReply reply = ping.Send("ipv6.google.com");
        return (reply.Status == IPStatus.Success);
    }
    catch
    {
        return false;
    }
}

// thanks GPT-3 for this code
// https://chat.openai.com/
double GetCpuUsage(double interval)
{
    PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    cpuCounter.NextValue();
    System.Threading.Thread.Sleep((int)interval);
    return cpuCounter.NextValue();
}
/*
class NetworkTrafficMonitor
{
    private PerformanceCounter bytesSentCounter;
    private PerformanceCounter bytesReceivedCounter;

    public long BytesSent { get; private set; }
    public long BytesReceived { get; private set; }

    public NetworkTrafficMonitor()
    {
        bytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", true);
        bytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", true);
    }

    public void StartMonitoring()
    {
        Console.WriteLine("Monitoring network traffic...");
        while (true)
        {
            var bytesSent = bytesSentCounter.NextValue();
            var bytesReceived = bytesReceivedCounter.NextValue();

            BytesSent += (long)bytesSent;
            BytesReceived += (long)bytesReceived;

            //Console.WriteLine("Bytes sent: {0}/s, Bytes received: {1}/s", bytesSent, bytesReceived);

            Thread.Sleep(1000);
        }
    }
}
*/