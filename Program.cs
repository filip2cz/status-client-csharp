using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;

if (debug)
{
    Console.WriteLine("Start");
}

bool debug = true;
bool auth = false;

if (debug)
{
    Console.WriteLine("Reading data from config");
}
// nastavení serveru a portu (todo: dát to do config souboru
string server = "status.fkomarek.eu";
int port = 35601;
string user = "user";
string passwd = "password";

while (true)
{
    if (debug)
    {
        Console.WriteLine("Creating TcpClient instance");
    }
    
    // vytvoření instance TCP klienta
    TcpClient client = new TcpClient();

    if (debug)
    {
        Console.WriteLine("Connecting to server...");
    }

    if (!client.Connected)
    {
        client.Connect(server, port);
        auth = true;
    }

    if (debug)
    {
        if (client.Connected)
        {
            Console.WriteLine("Connected");
        }
        else if (!client.Connected)
        {
            Console.WriteLine("Connection aborted");
        }
    }

    // získání streamu pro odesílání dat
    NetworkStream streamLogin = client.GetStream();

    byte[] buffer = new byte[1024];
    int bytesRead = 0;

    /*
    if (client.Connected)
    {
        Console.WriteLine("client is still connected btw.");
    }
    else
    {
        Console.WriteLine("client is not connectd btw.");
    }
    */

    if (auth)
    {
        // přečte data ze serveru
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        // autentizace
        string login = $"{user}:{passwd}\r\n";
        byte[] loginData = Encoding.ASCII.GetBytes(login);
        if (debug)
        {
            Console.WriteLine($"sending: {login}");
        }
        streamLogin.Write(loginData, 0, loginData.Length);

        // přečte data ze serveru
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        // přečte data ze serveru
        bytesRead = streamLogin.Read(buffer, 0, buffer.Length);
        Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

        auth = false;
    }

    // sending
    while (client.Connected)
    {
        string data = "update {\"online6\": false,  \"uptime\": 229, \"load\": 0.93, \"memory_total\": 12200460, \"memory_used\": 2499944, \"swap_total\": 12201980, \"swap_used\": 0, \"hdd_total\": 3761418, \"hdd_used\": 1161319, \"cpu\": 19.0, \"network_rx\": 0, \"network_tx\": 0 }\r\n";
        byte[] dataSend = Encoding.ASCII.GetBytes(data);

        NetworkStream stream = client.GetStream();
        stream.Write(dataSend, 0, dataSend.Length);

        Console.WriteLine(data);
        Thread.Sleep(3000);
    }
    
    // disconnect
    client.Close();
    client.Dispose();
}