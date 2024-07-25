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
                new Client("Azat", 12346, serverIPEndPoint).Run();
            }
            else if (args.Length == 1)
            {
                new Client(args[0], 123456, serverIPEndPoint).Run();
            }
            else if (args.Length == 2)
            {
                if (int.TryParse(args[1], out int Port))
                    new Client(args[0], Port, serverIPEndPoint).Run();
                else
                    Console.WriteLine($"Не удалось преобразовать строку {args[1]} в номер порта.");
            }
            else
            {
                Console.WriteLine("Запустите клиент с использованием двух аргументов: имени пользователя и номера порта.");
            }
        }
    }
}