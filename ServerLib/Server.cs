using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AbstractClientLib;
using MessageLib;

namespace ServerLib
{
    public class Server : AbstractClient
    {
        public HashSet<string> ClientsSet { get => clients.Keys.ToHashSet(); }
        private readonly CancellationTokenSource serverStopTokenSource = new CancellationTokenSource();
        private Dictionary<string, ClientRecord> clients = [];
        private ConcurrentQueue<(Message, IPEndPoint)> rawMessages = [];
        public Server(int receiverPort) : base(receiverPort)
        {
            Console.WriteLine("Сервер инициализирован с адресом:");
            Console.WriteLine($"{LocalAddress}:{ListeningPort}");
        }
        public void Run()
        {
            Console.WriteLine("Сервер запущен.");
            Task.Run(() => base.StartMessageReceivingAsync((msg, ip) => { rawMessages.Enqueue((msg, ip)); }));
            Task.WaitAll([Task.Run(CancellationRequest), Task.Run(WorkingCycle)]);
        }
        private void CancellationRequest()
        {
            Console.WriteLine("Нажмите любую клавишу для того, чтобы завершить работу.");
            //Console.ReadKey();
            // TODO: Вернуть к ReadKey()
            Thread.Sleep(new TimeSpan(1, 0, 0));
            Console.WriteLine();
            Console.WriteLine("Работа сервера будет завершена после получения следующего сообщения.");
            serverStopTokenSource.Cancel();
        }
        private void WorkingCycle()
        {
            while (true && !serverStopTokenSource.IsCancellationRequested)
            {
                if (rawMessages.TryDequeue(out var tuple))
                {
                    HandleMessage(tuple.Item1, tuple.Item2);
                }
            }
            base.StopMessageReceiving();
        }
        private async Task SendServerMessageAsync(string message, string to, IPEndPoint ip)
        {
            await SendMessageAsync(new Message(message, "server", to, ListeningPort), ip);
        }
        private async Task SendAnswerAsync(Message message, IPEndPoint ip, string answer)
        {
            await SendServerMessageAsync(answer, message.Sender, new IPEndPoint(ip.Address, message.AnswerPort));
        }
        private async Task SendConfirmationAsync(Message message, IPEndPoint ip)
        {
            await SendAnswerAsync(message, ip, "Message delivered.");
        }
        private void HandleMessage(Message message, IPEndPoint ip)
        {
            Console.WriteLine($"С IP адреса {ip.Address}:{ip.Port} получено сообщение:");
            Console.WriteLine($"\t{message}");
            ClientRecord from = ClientRecord.GetFromMessageAndIP(message, ip);
            RegisterClient(message, from);
        }
        private void RegisterClient(Message message, ClientRecord from)
        {
            if (message.Receiver.ToLower().Equals("server") && message.Text.ToLower().Equals("register"))
            {
                if (this.clients.ContainsKey(from.AliasName))
                {
                    if (from.Equals(this.clients[from.AliasName]))
                    {
                        Task.Run(() => SendAnswerAsync(message, from.ListeningIP, "Client is already registered."));
                    }
                    else Task.Run(() => SendAnswerAsync(message, from.ListeningIP,
                        $"Client with name \"{message.Sender}\" is already registered with another IP. Change your name and try again."));
                }
                else
                {
                    if (from.AliasName.Equals("server") || from.AliasName.Equals("public")) 
                        Task.Run(() => SendAnswerAsync(message, from.ListeningIP, $"Name {message.Sender} is unacceptable."));
                    else
                    {
                        this.clients.Add(from.AliasName, from);
                        Task.Run(() => SendAnswerAsync(message, from.ListeningIP, $"You are registered with IP {from.ListeningIP.Address}:{from.ListeningIP.Port}."));
                        Console.WriteLine($"Client {message.Sender} is registered with IP {from.ListeningIP.Address}:{from.ListeningIP.Port}.");
                    }
                }
            }
            else VerifyClient(message, from);
        }
        private void VerifyClient(Message message, ClientRecord from)
        {
            if (clients.ContainsKey(from.AliasName))
            {
                
                if (this.clients[from.AliasName].Equals(from)) TransitPublicMessage(message, from);
                else Task.Run(() => SendAnswerAsync(message, from.ListeningIP,
                    $"Client with name \"{message.Sender}\" is already registered with another IP."
                    + "Change your name and try again."));
            }
            else Task.Run(() => SendAnswerAsync(message, from.ListeningIP, "Client was not registered."));
        }
        private void TransitPublicMessage(Message message, ClientRecord from)
        {
            if (message.Receiver.ToLower() == "public") Task.Run(() => SendPublicMessage(message, from));
            else TransitPersonalMessage(message, from);
        }
        private void TransitPersonalMessage(Message message, ClientRecord from)
         {
            string receiver = message.Receiver.ToLower();
            if (this.clients.TryGetValue(receiver, out ClientRecord? to)) SendPersonalMessage(message, from, to);
            else Task.Run(() => SendAnswerAsync(message, from.ListeningIP, $"There is no registered client with name \"{message.Receiver}\"."));
         }
        private void SendPublicMessage(Message message, ClientRecord from)
         {
            foreach (var c in this.clients)
            {
                if (!c.Value.Equals(from))
                {
                    Task.Run(() => SendMessageAsync(message, c.Value.ListeningIP).Wait());
                }
            }
            Task.Run(() => SendConfirmationAsync(message, from.ListeningIP));
        }
        private void SendPersonalMessage(Message message, ClientRecord from, ClientRecord to)
        {
            Task.Run(() => SendMessageAsync(message, to.ListeningIP));
            Task.Run(() => SendConfirmationAsync(message, from.ListeningIP));
        }
    }
}