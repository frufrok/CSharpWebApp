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
                Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine("Работа сервера будет завершена после получения следующего сообщения.");
                cancellationTokenSource.Cancel();
            }
            void cycle()
            {
                while (true && !cancellationToken.IsCancellationRequested)
                {
                    var message = ReceiveMessage(new IPEndPoint(IPAddress.Any, 0));
                    Console.WriteLine(GetMessageReceivedText(message));
                    SendConfirmation(message);
                }
            }
            var serverTask = new Task(cycle, cancellationToken);
            List<Task> tasks = [Task.Run(cycle), Task.Run(exit)];
            Task.WaitAll(tasks.ToArray());
        }
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken cancellationToken;
        private static string GetMessageReceivedText(Message message)
        {
            return $"{message.DateTime}: Получено сообщение от \"{message.From}\" к \"{message.To}\" с текстом: \"{message.Text}\".";
        }
        private void SendConfirmation(Message message)
        {
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Parse(message.SenderIP), message.SenderPort);
            if (senderEndPoint != null)
            {
                var answer = new Message("Message delivered.", "Server", message.From, this.IPEndPoint.Address.ToString(), this.IPEndPoint.Port);
                this.SendMessage(answer, senderEndPoint);
            }
        }
    }
}