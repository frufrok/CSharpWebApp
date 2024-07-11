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
            Console.WriteLine("Сервер инициализирован с IP адресом:");
            Console.WriteLine(this.IPEndPoint.Address.ToString());
            Console.WriteLine("Номер порта:");
            Console.WriteLine(this.IPEndPoint.Port.ToString());
        }

        public void Run()
        {
            Console.WriteLine("Сервер запущен.");
            while (true)
            {
                var message = ReceiveMessage(new IPEndPoint(IPAddress.Any, 0));
                Console.WriteLine(GetMessageReceivedText(message));
                SendConfirmation(message);
            }
        }
       
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