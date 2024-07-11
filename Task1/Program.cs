using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Task1
{
   public class Program
   {
        public static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                Server server = new Server(12345);
                Client client = new Client(12346, server.IPEndPoint);
                Thread serverThread = new Thread(() => RunServer(server));
                Thread clientThread = new Thread(() => RunClient(client, args[0], args[1]));
                serverThread.Start();
                clientThread.Start();
            }
          }
        private static void RunServer(Server server)
        {
            server.Run();
        }
        private static void RunClient(Client client, string from, string to)
        {
            client.Run(from, to);
        }
    }
}