using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AbstractClientLib;
using MessageLib;

namespace ClientLib
{
    public class Client : AbstractClient
    {
        public string Name { get; init; }
        public IPEndPoint? ServerEndPoint { get; init; }
        private ConcurrentQueue<Message> rawMessages = [];
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
            Task.Run(() => base.StartMessageReceivingAsync((msg, ip) => { this.rawMessages.Enqueue(msg); }));
            Console.WriteLine("Запущено прослушивание входящих сообщений.");
            Task.Run(this.PrintInbox);
            Console.WriteLine("Запущен вывод входящих сообщений в консоль.");
            if (RegisterClient()) StartWorkingCycle();
            else Console.WriteLine("Регистрация клиента не удалась. Работа программы завершена.");
        }
        private bool RegisterClient()
        {
            Console.WriteLine("Регистрация клиента");
            if (ServerEndPoint != null)
            {
                int lastMessageNumber = base.InBox.Count;
                Task.Run(() => SendMessageAsync("server", "register"));
                int k = 0;
                while(k++ < 10)
                {
                    int lastI = 0;
                    for (int i = lastMessageNumber; i < base.InBox.Count; i++)
                    {
                        var msg = base.InBox.ElementAt(i).Item1;
                        if (msg.From.ToLower().Equals("server") && msg.Text.ToLower().Contains("you are registered with ip"))
                        {
                            Console.WriteLine("Регистрация успешна.");
                            return true;
                        }
                        lastI = i;
                    }
                    lastMessageNumber = lastI;
                    Console.WriteLine(".");
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
                var msg = new Message(text, this.Name, to, ListeningPort);
                await base.SendMessageAsync(msg, ServerEndPoint);
            }
            else
            {
                Console.WriteLine("EndPoint сервера недоступен.");
            }
        }
        private void PrintInbox()
        {
            while(true)
            {
                if (this.rawMessages.TryDequeue(out var msg))
                {
                    Console.WriteLine(msg);
                }
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