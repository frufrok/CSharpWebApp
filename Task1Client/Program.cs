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
            var serverIPEndPoint = new IPEndPoint(AbstractClient.GetLocalIPAddress(), 12345);
            if (args.Length == 0)
            {
                new Client("Azat", 0, serverIPEndPoint).Run();
            }
            else if (args.Length == 1)
            {
                new Client(args[0], 0, serverIPEndPoint).Run();
            }
            else
            {
                Console.WriteLine("Запустите клиент с использованием одного аргумента – имени пользователя.");
            }
        }
    }
}