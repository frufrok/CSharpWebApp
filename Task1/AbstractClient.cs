using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Task1;

namespace Task1
{
    public abstract class AbstractClient
    {
        public int ReceiverPort { get; set; }
        public IPEndPoint IPEndPoint { get; init; }
        public UdpClient UdpClient { get; init; }
        public AbstractClient(int receiverPort)
        {
            var ipString = GetLocalIPAddress();
            if (IPAddress.TryParse(ipString, out IPAddress? ip))
            {
                this.IPEndPoint = new IPEndPoint(ip, receiverPort);
            }
            else
            {
                throw new Exception($"Can't parse ip address from string \"{ipString}\".");
            }
            this.ReceiverPort = this.IPEndPoint.Port;
            UdpClient = new UdpClient(receiverPort);
            if (UdpClient.Client.LocalEndPoint != null)
            {
                this.IPEndPoint.Port = ((IPEndPoint)UdpClient.Client.LocalEndPoint).Port;
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        protected Message? ReceiveMessage(IPEndPoint senderEndPoint, int remainingTime)
        {
            while (true)
            {
                byte[]? buffer = null;
                object lockObj = new object();
                var receiveTask = Task.Run(() =>
                {
                    try
                    {
                        buffer = this.UdpClient.Receive(ref senderEndPoint);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Нет ответа от сервера.");
                    }
                });
                var remaining = Task.Run(() => Thread.Sleep(remainingTime));
                Task.WaitAny([receiveTask, remaining]);
                lock (lockObj)
                {
                    if (buffer != null)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer);
                        Message? message = Message.DeserializeFromJson(messageJson);
                        if (message != null) return message;
                    }
                    else
                    {
                        UdpClient.Close();
                        receiveTask.Wait();
                        return null;
                    }
                }
            }
        }
        protected void SendMessage(Message message, IPEndPoint receiverEndPoint)
        {
                string json = message.SerializeToJson();
                var data = Encoding.UTF8.GetBytes(json);
                new UdpClient().Send(data, data.Length, receiverEndPoint);
        }
    }
}