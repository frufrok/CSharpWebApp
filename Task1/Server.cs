using System;
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
        public Server(int receiverPort) : base(receiverPort)
        {
            cancellationToken = cancellationTokenSource.Token;
            Console.WriteLine("Сервер инициализирован с IP адресом:");
            Console.WriteLine(this.IPEndPoint.Address.ToString());
            Console.WriteLine("Номер порта:");
            Console.WriteLine(this.IPEndPoint.Port.ToString());
        }

        public void Run()
        {
            Console.WriteLine("Сервер запущен.");
            void exit()
            {
                Console.WriteLine("Нажмите любую клавишу для того, чтобы завершить работу.");
                //Console.ReadKey();
                // TODO: Вернуть к ReadKey()
                Thread.Sleep(new TimeSpan(1, 0, 0));
                Console.WriteLine();
                Console.WriteLine("Работа сервера будет завершена после получения следующего сообщения.");
                cancellationTokenSource.Cancel();
            }
            void cycle()
            {
                while (true && !cancellationToken.IsCancellationRequested)
                {
                    var message = ReceiveMessage(new IPEndPoint(IPAddress.Any, 0), 5000);
                    if (message != null) 
                    {
                        Console.WriteLine(GetMessageReceivedText(message));
                        HandleMessage(message);
                        SendConfirmation(message);
                    }
                }
            }
            var serverTask = new Task(cycle, cancellationToken);
            List<Task> tasks = [Task.Run(cycle), Task.Run(exit)];
            Task.WaitAll(tasks.ToArray());
        }
        private Dictionary<string, IPEndPoint> clients = [];
        private void HandleMessage(Message message)
        {
            RegisterClient(message);
        }
        private void RegisterClient(Message message)
        {
            string sender = message.From.ToLower();
            if (message.To.ToLower().Equals("server") && message.Text.ToLower().Equals("register"))
            {
                if (this.clients.ContainsKey(sender))
                {
                    if (this.clients[sender].Address.ToString().Equals(message.SenderIP)
                        && this.clients[sender].Port.Equals(message.SenderPort))
                    {
                        SendAnswer(message, "Client is already registered.");
                    }
                    else SendAnswer(message, $"Client with name \"{message.From}\" is already registered with another IP. Change your name and try again.");
                }
                else
                {
                    if (sender.Equals("server") || sender.Equals("public")) SendAnswer(message, $"Name {message.From} is unacceptable.");
                    else 
                    {
                        this.clients.Add(sender, GetSenderIPEndPoint(message));
                        string text = $"Client {message.From} is registered with IP{message.SenderIP} on port {message.SenderPort}.";
                        SendAnswer(message, $"OK. " + text);
                        Console.WriteLine(text);
                    }
                }
            }
            else VerifyClient(message);
        }
        private void VerifyClient(Message message)
        {
            if (clients.ContainsKey(message.From.ToLower()))
            {
                if (this.clients[message.From.ToLower()].Address.ToString() == message.SenderIP
                    && this.clients[message.From.ToLower()].Port == message.SenderPort)
                {
                    TransitPublicMessage(message);
                }
                else SendAnswer(message, $"Client with name \"{message.From}\" is already registered with another IP. Change your name and try again.");
            }
            else
            {
                SendAnswer(message, "Client was not registered.");
            }
        }
        private void TransitPublicMessage(Message message)
        {
            if (message.To.ToLower() == "public") SendVerifiedPublicMessage(message);
            else TransitPersonalMessage(message);
        }
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
        public HashSet<string> ClientsSet => clients.Keys.ToHashSet();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken cancellationToken;
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
        private IPEndPoint GetSenderIPEndPoint(Message message)
        {
            return new IPEndPoint(IPAddress.Parse(message.SenderIP), message.SenderPort);
        }
        private void SendConfirmation(Message message)
        {
            SendAnswer(message, "Message delivered.");
        }
    }
}