using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServer;

internal class Program
{
    static void Main(string[] args)
    {
        CurrencyServer server = new CurrencyServer();
        server.Start();
    }
}


class CurrencyServer
{
    private TcpListener _listenter;
    private bool _isRunning;

    private static readonly Dictionary<string, double> ExcangeRates = new Dictionary<string, double>()
    {
        {"USD_EURO",0.91},
        {"EURO_USD",1.10},
        {"USD_UAH",41.25},
        {"UAH_USD",0.025},
        {"USD_GBP",0.76 },
        {"GBP_USD",1.32 }

    };

    public CurrencyServer()
    {
        _listenter = new TcpListener(IPAddress.Any, 8888);
        _isRunning = true;
    }

    public void Start()
    {
        _listenter.Start();
        Console.WriteLine("The server is running and waiting to connect...");

        while (_isRunning)
        {
            TcpClient client = _listenter.AcceptTcpClient();
            Console.WriteLine("The client is connected");

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }


    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;

        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Client Request: {request}");

            string[] currencies = request.Split(' ');
            if (currencies.Length == 2)
            {
                string currencyPair = $"{currencies[0]}_{currencies[1]}";
                if (ExcangeRates.ContainsKey(currencyPair))
                {
                    double rate = ExcangeRates[currencyPair];
                    string response = rate.ToString();
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine($"Server response: {response}");
                }
                else
                {
                    string error = "Error: Course not found";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                    stream.Write(errorBytes, 0, errorBytes.Length);
                    Console.WriteLine($"Ответ сервера: Курс не найден");
                }
            }
            else 
            {
                string error = "Error: I don't understand the message";
                byte[] errorBytes = Encoding.UTF8.GetBytes(error);
                stream.Write(errorBytes, 0, errorBytes.Length);
                Console.WriteLine("Server response: I don't understand the message");
            }
        }
    }
}




