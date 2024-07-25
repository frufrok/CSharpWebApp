using System;
using System.Collections.Concurrent;
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
        protected UdpClient SendingUdpClient { get; init; }
        protected UdpClient ListeningUdpClient { get; init; }
        protected int ListeningPort { get; init; }
        protected IPAddress LocalAddress { get; init; }
        protected BlockingCollection<(Message, IPEndPoint)> InBox { get; init; } = [];
        private CancellationTokenSource StopReceivingTokenSource = new CancellationTokenSource();
        public AbstractClient(int receiverPort)
        {
            this.LocalAddress = GetLocalIPAddress();
            if (receiverPort > 0) 
            {
                this.ListeningUdpClient = new UdpClient(receiverPort);
                this.ListeningPort = receiverPort;
            }
            else
            {
                this.ListeningUdpClient = new UdpClient();
                if (this.ListeningUdpClient.Client.LocalEndPoint != null)
                    this.ListeningPort = ((IPEndPoint)this.ListeningUdpClient.Client.LocalEndPoint).Port;
                else
                    throw new Exception("Не удалось получить номер порта для получения сообщений.");                
            }
            this.SendingUdpClient = new UdpClient();
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
                var result = await ListeningUdpClient.ReceiveAsync();
                var messageJson = Encoding.UTF8.GetString(result.Buffer);
                Message? message = Message.DeserializeFromJson(messageJson);
                if (message != null)
                {
                    PreliminaryHandling(message, result.RemoteEndPoint);
                    InBox.Add((message, result.RemoteEndPoint));
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
            await SendingUdpClient.SendAsync(data, data.Length, receiverEndPoint);
        }
    }
}