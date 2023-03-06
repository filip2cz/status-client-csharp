using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;

bool running = true;
bool debug = true;

// nastavení serveru a portu (todo: dát to do config souboru
string server = "status.fkomarek.eu";
int port = 35601;

string user = "user";
string passwd = "password";

while (running)
{
    // vytvoření instance TCP klienta
    TcpClient client = new TcpClient(server, port);

    // získání streamu pro odesílání dat
    NetworkStream stream = client.GetStream();

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

    // přečte data ze serveru
    bytesRead = stream.Read(buffer, 0, buffer.Length);
    Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

    Thread.Sleep(1000);

    // autentizace
    string login = $"{user}:{passwd}\r\n";
    byte[] loginData = Encoding.ASCII.GetBytes(login);
    if (debug)
    {
        Console.WriteLine($"sending: {login}");
    }
    stream.Write(loginData, 0, loginData.Length);

    // přečte data ze serveru
    bytesRead = stream.Read(buffer, 0, buffer.Length);
    Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

    // přečte data ze serveru
    bytesRead = stream.Read(buffer, 0, buffer.Length);
    Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));

    while (client.Connected)
    {
        string data = "lol";
        byte[] dataSend = Encoding.ASCII.GetBytes(data);
        stream.Write(dataSend, 0, dataSend.Length);
        Thread.Sleep(1000);
    }

    client.Close();

    running = false;
}