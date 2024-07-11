using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Task1
{
    public class Server
    {
        public void WaitForMessage()
        {
            UdpClient udpClient = new UdpClient(12345);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Сервер ждет сообщение от клиента.");
            while(true)
            {
                byte[] buffer = udpClient.Receive(ref iPEndPoint);
                var messageJson = Encoding.UTF8.GetString(buffer);

                Message? message = Message.DeserializeFromJson(messageJson);
                if (message != null) PrintGetMsgLog(message);
            }
        }
        public void PrintGetMsgLog(Message message)
        {
            Console.WriteLine($"{message.DateTime}: Получено сообщение от \"{message.From}\" к \"{message.To}\" с текстом: \"{message.Text}\".");
        }
    }
}