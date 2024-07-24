using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Task1;

namespace Task1Client
{
    public class Client : AbstractClient
    {
        public string Name { get; init; }
        public IPEndPoint? ServerEndPoint { get; init; }
        public Client(string Name, int receiverPort, IPEndPoint serverEndPoint) : base(receiverPort)
        {
            this.Name = Name;
            ServerEndPoint = serverEndPoint;
            Console.WriteLine("Клиент инициализирован c IP адресом:");
            Console.WriteLine(this.IPEndPoint.Address.ToString());
            Console.WriteLine("Номер порта:");
            Console.WriteLine(this.IPEndPoint.Port.ToString());
            Console.WriteLine("IP адрес сервера:");
            Console.WriteLine(ServerEndPoint?.Address.ToString());
        }
        private void RegisterClient()
        {
            if (ServerEndPoint != null)
            {
                SendMessage("server", "register", out Message? answer);
                if (answer != null && answer.Text.ToLower().StartsWith("ok"))
                {
                    Console.WriteLine("Client is registered.");
                }
                else 
                {
                    throw new Exception("Client is not registered.");
                }
            }
            else
            {
                throw new Exception("Server IPEndPoint is null. Cannot register client.");
            }
        }
        public void Run()
        {
            Console.WriteLine("Клиент запущен.");
            RegisterClient();
            while (true)
            {
                while (true)
                {
                    string? text;
                    string? toName;
                    bool exitFlag;
                    do
                    {
                        Console.WriteLine("Введите адресата сообщения или введите 'public', чтобы написать в общий чат:");
                        toName = Console.ReadLine();
                        exitFlag = toName != null && toName.ToLower().Equals("exit");
                    }
                    while (string.IsNullOrEmpty(toName) && !exitFlag);
                    do
                    {
                        Console.WriteLine("Введите сообщение:");
                        text = Console.ReadLine();
                        exitFlag = text!= null && text.ToLower().Equals("exit");
                    }
                    while (string.IsNullOrEmpty(text) && !exitFlag);
                    if (exitFlag)
                    {
                        Console.WriteLine("Работа клиента завершена.");
                        break;
                    }
                    else SendPersonalMessage(toName??String.Empty, text??String.Empty);
                }
            }
        }
        private void SendMessage(string to, string text, out Message? answer)
        {
            if (ServerEndPoint != null)
            {
                var msg = new Message(text, this.Name, to, this.IPEndPoint.Address.ToString(), this.IPEndPoint.Port);
                SendMessage(msg, ServerEndPoint);
                answer = ReceiveMessage(ServerEndPoint, 5000);
            }
            else
            {
                Console.WriteLine("EndPoint сервера недоступен.");
                answer = null;
            }
        }
        private void SendPersonalMessage(string to, string text)
        {
            SendMessage(to, text, out Message? answer);
            if (answer != null && answer.Text.Equals("Message delivered.")) Console.WriteLine("Message delivered.");
            else Console.WriteLine("Message delivery confirmation doesn't received.");
        }
    }
}