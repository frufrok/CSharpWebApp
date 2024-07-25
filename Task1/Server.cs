using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    public class Server : AbstractClient
    {
        public HashSet<string> ClientsSet { get => clients.Keys.ToHashSet(); }
        private readonly CancellationTokenSource serverStopTokenSource = new CancellationTokenSource();
        private Dictionary<string, IPEndPoint> clients = [];
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
        private void HandleMessage(Message message, IPEndPoint ip)
        {
            Console.WriteLine($"С IP адреса {ip.Address}:{ip.Port} получено сообщение:");
            Console.WriteLine($"\t{message}");
            RegisterClient(message, ip);
        }
      
        private void RegisterClient(Message message, IPEndPoint ip)
        {
            string sender = message.From.ToLower();
            if (message.To.ToLower().Equals("server") && message.Text.ToLower().Equals("register"))
            {
                if (this.clients.ContainsKey(sender))
                {
                    if (this.clients[sender].Equals(ip))
                    {
                        Task.Run(() => SendAnswerAsync(message, ip, "Client is already registered."));
                    }
                    else Task.Run(() => SendAnswerAsync(message, ip,
                        $"Client with name \"{message.From}\" is already registered with another IP. Change your name and try again."));
                }
                else
                {
                    if (sender.Equals("server") || sender.Equals("public")) 
                        Task.Run(() => SendAnswerAsync(message, ip, $"Name {message.From} is unacceptable."));
                    else
                    {
                        this.clients.Add(sender, ip);
                        Task.Run(() => SendAnswerAsync(message, ip, $"register is ok"));
                        Console.WriteLine($"Client {message.From} is registered with IP {ip.Address}:{ip.Port}.");
                    }
                }
            }
            else VerifyClient(message, ip);
        }
        public async Task SendServerMessageAsync(string message, string to, IPEndPoint ip)
        {
            await SendMessageAsync(new Message(message, "server", to, ListeningPort), ip);
        }
        public async Task SendAnswerAsync(Message message, IPEndPoint ip, string answer)
        {
            await SendServerMessageAsync(answer, message.From, new IPEndPoint(ip.Address, message.AnswerPort));
        }
        
        private void VerifyClient(Message message, IPEndPoint ip)
        {
            Console.WriteLine("Верификация клиента");
            if (clients.ContainsKey(message.From.ToLower()))
            {
                if (this.clients[message.From.ToLower()].Equals(ip)) TransitPublicMessage(message);
                else Task.Run(() => SendAnswerAsync(message, ip,
                    $"Client with name \"{message.From}\" is already registered with another IP."
                    + "Change your name and try again."));
            }
            else Task.Run(() => SendAnswerAsync(message, ip, "Client was not registered."));
        }
        
        private void TransitPublicMessage(Message message)
        {
            Console.WriteLine("Публичное сообщение");
            //if (message.To.ToLower() == "public") SendVerifiedPublicMessage(message);
            //else TransitPersonalMessage(message);
        }
        /*
         private void TransitPersonalMessage(Message message)
         {
             string receiver = message.To.ToLower();
             if (this.clients.ContainsKey(receiver)) SendVerifiedPersonalMessage(message);
             else SendAnswer(message, $"There is no registered client with name \"{message.To}\".");
         }
         private void SendVerifiedPublicMessage(Message message)
         {
             foreach (var c in this.clients)
             {
                 if (c.Key != message.From.ToLower())
                 {
                     SendMessage(message, c.Value);
                 }
             }
             SendConfirmation(message);
         }
         private void SendVerifiedPersonalMessage(Message message)
         {
             SendMessage(message, this.clients[message.To.ToLower()]);
             SendConfirmation(message);
         }
         private static string GetMessageReceivedText(Message message)
         {
             return $"{message.DateTime}: Получено сообщение от \"{message.From}\" к \"{message.To}\" с текстом: \"{message.Text}\".";
         }
         private void SendAnswer(Message message, string answerText)
         {
             IPEndPoint senderEndPoint = GetSenderIPEndPoint(message);
             if (senderEndPoint != null)
             {
                 var answer = new Message(answerText, "Server", message.From, this.IPEndPoint.Address.ToString(), this.IPEndPoint.Port);
                 this.SendMessage(answer, senderEndPoint);
             }
         }
         private void SendConfirmation(Message message)
         {
             SendAnswer(message, "Message delivered.");
         }
         */
    }
}