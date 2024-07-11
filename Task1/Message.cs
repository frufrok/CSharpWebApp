using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Task1
{
    public class Message(string text, string from, string to, string senderIP, int senderPort)
    {
        public string Text { get; set; } = text;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string SenderIP { get; set; } = senderIP;
        public int SenderPort { get; set; } = senderPort;
        public string From { get; set; } = from;
        public string To { get; set; } = to;
        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public static Message? DeserializeFromJson(string json)
        {
            return JsonSerializer.Deserialize<Message>(json);
        }
        public override bool Equals(object? obj)
        {
            if (obj?.GetType() == typeof(Message))
            {
                var that = (Message)obj;
                return String.Equals(this.Text, that.Text)
                && String.Equals(this.From, that.From)
                && String.Equals(this.To, that.To)
                && DateTime.Equals(this.DateTime, that.DateTime)
                && String.Equals(this.SenderIP, that.SenderIP)
                && int.Equals(this.SenderPort, that.SenderPort);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Text, this.From, this.To, this.DateTime, this.SenderIP, this.SenderPort);
        }
    }
}