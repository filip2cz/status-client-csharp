using Newtonsoft.Json;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

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
        var hdd = GetHddInfo();
        var network = GetNetworkUsage();

        string data = "update {\"online6\": " + CheckIPv6Support() + ",  \"uptime\": " + GetUptime() + ", \"load\": -1.0, \"memory_total\": " + memory.ramTotal + ", \"memory_used\": " + (memory.ramTotal - memory.ramFree) + ", \"swap_total\": " + memory.swapTotal + ", \"swap_used\": " + (memory.swapTotal - memory.swapFree) + ", \"hdd_total\": " + hdd.total + ", \"hdd_used\": " + hdd.used + ", \"cpu\": " + GetCpuUsage() + ".0, \"network_rx\": " + network.rx + ", \"network_tx\": " + network.tx + " }\r\n";
        Console.WriteLine($"Main(): data = {data}");
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
        // Thread.Sleep(3000);
        // GetCpuUsage has sleep in it
    }

    Console.WriteLine("Client disconnected, connecting again");
    Debug.WriteLine("Main(): Client disconnected, connecting again");
    Thread.Sleep(1000);
}
static string CheckConfig()
{
    if (File.Exists("config.json") == true)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Debug.WriteLine($"CheckConfig(): Config found at {Environment.CurrentDirectory}\\config.json");
            return $"{Environment.CurrentDirectory}\\config.json";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Debug.WriteLine($"CheckConfig(): Config found at {Environment.CurrentDirectory}/config.json");
            return $"{Environment.CurrentDirectory}/config.json";
        }
        else
        {
            return "";
        }
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
        if (reply.Status == IPStatus.Success)
        {
            return "true";
        }
        else
        {
            return "false";
        }
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
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
    // thanks GPT-5 for this code
    // https://chatgpt.com/
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        double totalMem = 0, freeMem = 0, swapTotal = 0, swapFree = 0;
        foreach (var line in File.ReadAllLines("/proc/meminfo"))
        {
            if (line.StartsWith("MemTotal:")) totalMem = ParseKb(line);
            else if (line.StartsWith("MemAvailable:")) freeMem = ParseKb(line);
            else if (line.StartsWith("SwapTotal:")) swapTotal = ParseKb(line);
            else if (line.StartsWith("SwapFree:")) swapFree = ParseKb(line);
        }

        var memory = new
        {
            ramTotal = (int)totalMem,
            ramFree = (int)freeMem,
            swapTotal = (int)swapTotal,
            swapFree = (int)swapFree
        };

        return memory;
    }
    else
    {
        return "";
    }
}
// thanks GPT-5 for this code
// https://chatgpt.com/
static double ParseKb(string line)
{
    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return double.Parse(parts[1]);
}
static dynamic GetHddInfo()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        DriveInfo driveC = new DriveInfo("C");
        var hdd = new
        {
            total = driveC.TotalSize / 1024 / 1024,
            used = (driveC.TotalSize - driveC.TotalFreeSpace) / 1024 / 1024
        };
        return hdd;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        string path = "/";

        DriveInfo drive = new DriveInfo(path);

        var hdd = new
        {
            total = (int)(drive.TotalSize / 1024 / 1024),
            used = (int)((drive.TotalSize - drive.AvailableFreeSpace) / 1024 / 1024)
        };
        return hdd;
    }
    else
    {
        var hdd = new
        {
            total = 0,
            used = 0
        };
        return hdd;
    }
}
static int GetCpuUsage()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        // thanks GPT-3 for this code
        // https://chat.openai.com/
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue();
        Thread.Sleep(2000); // sleep so this works
        return (int)cpuCounter.NextValue();
    }
    // thanks GPT-5 for this code
    // https://chatgpt.com/
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        var cpu1 = File.ReadAllText("/proc/stat").Split("\n")[0];
        var parts1 = cpu1.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        ulong user1 = ulong.Parse(parts1[1]);
        ulong nice1 = ulong.Parse(parts1[2]);
        ulong system1 = ulong.Parse(parts1[3]);
        ulong idle1 = ulong.Parse(parts1[4]);

        Thread.Sleep(2000);

        var cpu2 = File.ReadAllText("/proc/stat").Split("\n")[0];
        var parts2 = cpu2.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        ulong user2 = ulong.Parse(parts2[1]);
        ulong nice2 = ulong.Parse(parts2[2]);
        ulong system2 = ulong.Parse(parts2[3]);
        ulong idle2 = ulong.Parse(parts2[4]);

        ulong total1 = user1 + nice1 + system1 + idle1;
        ulong total2 = user2 + nice2 + system2 + idle2;

        ulong totalDelta = total2 - total1;
        ulong idleDelta = idle2 - idle1;

        double cpuUsage = (double)(totalDelta - idleDelta) / totalDelta * 100;

        return (int)cpuUsage;
    }
    else
    {
        Thread.Sleep(2000);
        return -1;
    }
}
static dynamic GetNetworkUsage()
{
    // network rx and tx
    // https://stackoverflow.com/questions/2081827/c-sharp-get-system-network-usage
    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

    int network_rx = 0;
    int network_tx = 0;

    foreach (NetworkInterface ni in interfaces)
    {
        network_rx = (int)ni.GetIPv4Statistics().BytesReceived;
        network_tx = (int)ni.GetIPv4Statistics().BytesSent;
    }

    var network = new
    {
        rx = network_rx / 1024,
        tx = network_tx / 1024
    };

    return network;
}