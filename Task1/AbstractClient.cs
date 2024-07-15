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
            this.ReceiverPort = receiverPort;
            var ipString = GetLocalIPAddress();
            UdpClient = new UdpClient(receiverPort);
            if (IPAddress.TryParse(ipString, out IPAddress? ip))
            {
                this.IPEndPoint = new IPEndPoint(ip, receiverPort);
            }
            else
            {
                throw new Exception($"Can't parse ip address from string \"{ipString}\".");
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
        protected Message ReceiveMessage(IPEndPoint senderEndPoint)
        {
            
            while (true)
            {
                byte[] buffer = this.UdpClient.Receive(ref senderEndPoint);
                var messageJson = Encoding.UTF8.GetString(buffer);

                Message? message = Message.DeserializeFromJson(messageJson);
                if (message != null) return message;
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