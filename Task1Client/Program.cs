using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Task1;

namespace Task1Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                SentMessage(args[0], args[1], args[2]);
            }
        }
        public static void SentMessage(string from, string to, string ip)
        {
            var udpClient = new UdpClient();
            var iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), 12345);

            while (true)
            {
                string? text;
                do
                {
                    Console.Clear();
                    Console.WriteLine("Введите сообщение:");
                    text = Console.ReadLine();
                }
                while (string.IsNullOrEmpty(text));
                Message message = new Message(text, from, to);
                string json = message.SerializeToJson();
                var data = Encoding.UTF8.GetBytes(json);
                udpClient.Send(data, data.Length, iPEndPoint);
            }
        }
    }
}