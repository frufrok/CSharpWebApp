using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Task1
{
    public class Client : AbstractClient
    {
        public IPEndPoint ServerEndPoint { get; init; }
        public Client(int receiverPort, IPEndPoint serverEndPoint) : base(receiverPort)
        {
            ServerEndPoint = serverEndPoint;
            Console.WriteLine("Клиент инициализирован c IP адресом:");
            Console.WriteLine(this.IPEndPoint.Address.ToString());
            Console.WriteLine("Номер порта:");
            Console.WriteLine(this.IPEndPoint.Port.ToString());
            Console.WriteLine("IP адрес сервера:");
            Console.WriteLine(ServerEndPoint?.Address.ToString());
        }
        public void Run(string from, string to)
        {
            Console.WriteLine("Клиент запущен.");
            while (true)
            {
                while (true)
                {
                    string? text;
                    do
                    {
                        Console.WriteLine("Введите сообщение:");
                        text = Console.ReadLine();
                    }
                    while (string.IsNullOrEmpty(text));
                    SendMessage(from, to, text);
                }
            }
        }
        private void SendMessage(string from, string to, string text)
        {
            var msg = new Message(text, from, to, this.IPEndPoint.Address.ToString(), this.IPEndPoint.Port);
            SendMessage(msg, ServerEndPoint);
            var answer = ReceiveMessage(ServerEndPoint);
            if (answer.Text.Equals("Message delivered."))
                Console.WriteLine("Message delivered.");
            else
                Console.WriteLine("Message delivery confirmation doesn't received.");
        }
    }
}