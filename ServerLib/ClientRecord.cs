using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MessageLib;

namespace ServerLib
{
    public class ClientRecord(string name, IPEndPoint listeningIP, IPEndPoint sendingIP)
    {
        public string Name { get; set; } = name;
        public IPEndPoint ListeningIP { get; set; } = listeningIP;
        public IPEndPoint SendingIP { get; set; } = sendingIP;
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, ListeningIP, SendingIP);
        }
        public override bool Equals(object? obj)
        {
            if (this.GetType() == obj?.GetType())
            {
                if (this == null && obj == null) return true;
                else
                {
                    var that = (ClientRecord)obj;
                    return this.Name.Equals(that.Name)
                        && this.ListeningIP.Equals(that.ListeningIP)
                        && this.SendingIP.Equals(that.SendingIP);
                }
            }
            else return false;
        }
        public static ClientRecord GetFromMessageAndIP(Message message, IPEndPoint ip)
        {
            return new ClientRecord(message.From, new IPEndPoint(ip.Address, message.AnswerPort), ip);
        }
    }
}