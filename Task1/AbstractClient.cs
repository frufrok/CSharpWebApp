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
        protected UdpClient SenderUdpClient { get; init; }
        protected UdpClient ListenerUdpClient { get; init; }
        private IPAddress LocalAddress { get; init; }
        protected List<KeyValuePair<Message, IPEndPoint>> InBox { get; init; } = [];
        private CancellationTokenSource StopReceivingTokenSource = new CancellationTokenSource();
        public AbstractClient(int receiverPort)
        {
            this.LocalAddress = GetLocalIPAddress();
            this.ListenerUdpClient = receiverPort > 0 ? new UdpClient(receiverPort) : new UdpClient();
            this.SenderUdpClient = new UdpClient();
        }
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip;
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        protected async Task StartMessageReceivingAsync(Action<Message, IPEndPoint> PreliminaryHandling)
        {
            var stop = StopReceivingTokenSource.Token;
            while (true && !stop.IsCancellationRequested)
            {
                var result = await ListenerUdpClient.ReceiveAsync();
                var messageJson = Encoding.UTF8.GetString(result.Buffer);
                Message? message = Message.DeserializeFromJson(messageJson);
                if (message != null)
                {
                    PreliminaryHandling(message, result.RemoteEndPoint);
                    InBox.Add(new(message, result.RemoteEndPoint));
                }
            }
        }
        protected void StopMessageReceiving()
        {
            this.StopReceivingTokenSource.Cancel();
        }
        protected async Task SendMessageAsync(Message message, IPEndPoint receiverEndPoint)
        {
            string json = message.SerializeToJson();
            var data = Encoding.UTF8.GetBytes(json);
            await SenderUdpClient.SendAsync(data, data.Length, receiverEndPoint);
        }
    }
}