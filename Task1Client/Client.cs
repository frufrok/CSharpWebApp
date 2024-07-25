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
            Console.WriteLine($"Запущен клиент {this.Name}.");
            Console.WriteLine("Адрес сервера:");
            Console.WriteLine($"{ServerEndPoint?.Address.ToString()}:{ServerEndPoint?.Port}");
        }
        public void Run()
        {
            Console.WriteLine("Клиент запущен.");
            Task.Run(() => base.StartMessageReceivingAsync((msg, ip) => { return; }));
            Console.WriteLine("Запущено прослушивание входящих сообщений.");
            if (RegisterClient()) StartWorkingCycle();
        }
        private bool RegisterClient()
        {
            Console.WriteLine("Регистрация клиента.");
            if (ServerEndPoint != null)
            {
                int lastMessageNumber = base.InBox.Count;
                Task.Run(() => SendMessageAsync("server", "register")).Wait();
                int k = 0;
                while(k++ < 10)
                {
                    for (int i = lastMessageNumber; i < base.InBox.Count; i++)
                    {
                        var msg = base.InBox[i].Key;
                        if (msg.From.ToLower().Equals("server") && msg.Text.ToLower().Equals("register is ok"))
                        {
                            Console.WriteLine("Регистрация успешна.");
                            return true;
                        }
                    }
                    lastMessageNumber = base.InBox.Count;
                    Console.Write(".");
                    Thread.Sleep(1000);
                }
                return false;
            }
            else
            {
                throw new Exception("Server IPEndPoint is null. Cannot register client.");
            }
        }
        private async Task SendMessageAsync(string to, string text)
        {
            if (ServerEndPoint != null)
            {
                var msg = new Message(text, this.Name, to);
                await base.SendMessageAsync(msg, ServerEndPoint);
            }
            else
            {
                Console.WriteLine("EndPoint сервера недоступен.");
            }
        }
        private void StartWorkingCycle()
        {
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
                        exitFlag = text != null && text.ToLower().Equals("exit");
                    }
                    while (string.IsNullOrEmpty(text) && !exitFlag);
                    if (exitFlag)
                    {
                        Console.WriteLine("Работа клиента завершена.");
                        break;
                    }
                    else Task.Run(() => SendMessageAsync(toName ?? String.Empty, text ?? String.Empty));
                }
            }
        }
    }
}