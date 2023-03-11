using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

Console.WriteLine("Status client C# v0.3");
Console.WriteLine("by Filip Komárek");
Console.WriteLine("Github: https://github.com/filip2cz/status-client-csharp");
Console.WriteLine("Gitea mirror: https://git.envs.net/filip2cz/status-client-csharp");

bool auth = false;
int processorCount = Environment.ProcessorCount;
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
        // uptime
        var uptimeMilliseconds = System.Environment.TickCount64;
        var uptimeSeconds = (long)(uptimeMilliseconds / 1000);

        // sending
        string data = "update {\"online6\": false,  \"uptime\": " + uptimeSeconds.ToString() + ", \"load\": -1.0, \"memory_total\": 0, \"memory_used\": 0, \"swap_total\": 0, \"swap_used\": 0, \"hdd_total\": 0, \"hdd_used\": 0, \"cpu\": 0.0, \"network_rx\": 0, \"network_tx\": 0 }\r\n";
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
        Thread.Sleep(3000);
    }

    // disconnect
    client.Close();
    client.Dispose();
    Console.WriteLine("Disconnected");
}