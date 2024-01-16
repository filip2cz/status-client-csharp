using System.Diagnostics;
using ssh_buttons_console_demo;

string ver = "v0.1";

Config configLoader = new Config();

string[] config = configLoader.LoadConfig();

Debug.WriteLine(config);

Ssh ssh = new Ssh();

foreach (string prvek in config)
{
    Debug.WriteLine(prvek);
}

Console.WriteLine("SSH Buttons - test console version");
Console.WriteLine($"Version {ver}");
Console.WriteLine("Created by Filip Komárek");
Console.WriteLine("https://github.com/filip2cz/ssh-buttons");
Console.WriteLine();

string input = "-1";

string hostname;
if (config[0] == "askUser")
{
    Console.Write("Server hostname: ");
    hostname = Console.ReadLine();
}
else
{
    hostname = config[0];
}

string username;
if (config[1] == "askUser")
{
    Console.Write("Username: ");
    username = Console.ReadLine();
}
else
{
    username = config[1];
}

Console.Write("Password: ");
string password = ShowStars();
Console.WriteLine();

string output = string.Empty;

input = "-1";
output = string.Empty;

while (input != "0")
{
    bool z = true;
    int inputMaxLenght = 1;
    int y = 2;
    while (z)
    {
        try
        {
            Console.WriteLine($"[{inputMaxLenght}] {config[y]}");
            inputMaxLenght++;
            y += 2;
        }
        catch (Exception)
        {
            z = false;
        }
    }
    inputMaxLenght--;
    y = default(int);

    Console.WriteLine("[0] exit");

    Console.Write("Choose command: ");
    input = Console.ReadLine();

    Debug.WriteLine($"input = {input}");
    Debug.WriteLine($"config.Length = {config.Length}");
    Debug.WriteLine($"inputMaxLenght = {inputMaxLenght}");

    if (input == "")
    {
        input = "0";
    }
    else if (!(int.TryParse(input, out int number)) || int.Parse(input) > inputMaxLenght)
    {
        Console.WriteLine("Invalid command");
    }
    else if (input == "0")
    {
        Console.WriteLine("Exiting");
        Debug.WriteLine("Exiting");
    }
    else if (int.Parse(input) <= inputMaxLenght)
    {
        Console.Clear();
        Console.WriteLine("Running command");
        output = ssh.Command(hostname, username, password, config[int.Parse(input)*2+1]);
        Console.WriteLine(output);
        Console.WriteLine("------------------------");
    }
}
static string ShowStars()
{
    string password = "";
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password += key.KeyChar;
            Console.Write("*");
        }
    } while (key.Key != ConsoleKey.Enter);

    return password;
}