using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

bool auth = false;

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


    return;
}

else
{
    // nastavení serveru a portu (todo: dát to do config souboru
    var configJson = File.ReadAllText("config.json");
    dynamic config = JsonConvert.DeserializeObject(configJson);

    string server = config.server;
    int port = config.port;
    string user = config.user;
    string passwd = config.passwd;
    bool debug = config.debug;

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
            // uptime
            var uptimeMilliseconds = System.Environment.TickCount64;
            var uptimeSeconds = (long)(uptimeMilliseconds / 1000);

            // sending

            string data = "update {\"online6\": false,  \"uptime\": " + uptimeSeconds.ToString() + ", \"load\": 0, \"memory_total\": 0, \"memory_used\": 0, \"swap_total\": 0, \"swap_used\": 0, \"hdd_total\": 0, \"hdd_used\": 0, \"cpu\": 0.0, \"network_rx\": 0, \"network_tx\": 0 }\r\n";
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
}