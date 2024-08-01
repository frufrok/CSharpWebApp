using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageLib
{
    public class Message(string text, string sender, string receiver, int answerPort)
    {
        public string Text { get; set; } = text;
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public string Sender { get; set; } = sender;
        public string Receiver { get; set; } = receiver;
        public int AnswerPort { get; set; } = answerPort;
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
                && String.Equals(this.Sender, that.Sender)
                && String.Equals(this.Receiver, that.Receiver)
                && DateTime.Equals(this.DateTime, that.DateTime)
                && int.Equals(this.AnswerPort, that.AnswerPort);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Text, this.Sender, this.Receiver, this.DateTime, this.AnswerPort);
        }

        public override string ToString()
        {
            return $"{DateTime} From: {Sender}. To: {Receiver}. Answer port: {AnswerPort}. Text: {Text}.";
        }
    }
}