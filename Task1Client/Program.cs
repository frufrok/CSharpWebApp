using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Task1;

namespace Task1Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                try
                {
                    var serverIPEndPoint = new IPEndPoint(IPAddress.Parse(AbstractClient.GetLocalIPAddress()), 12345);
                    Client client = new Client(args[0], 0, serverIPEndPoint);
                    client.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка чтения IP-адреса: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Запустите клиент с использованием одного аргумента – имени пользователя.");
            }
        }
    }
}