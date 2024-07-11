using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Task1
{
   public class Program
   {
        public static void Main(string[] args)
        {
            Task1();
            Task2();
        }

        public static void Task1()
        {
            var msg = new Message("Hello", "Azat", "Gabil");
            string msgJson = msg.SerializeToJson();
            var msgCopy = Message.DeserializeFromJson(msgJson);
            Console.WriteLine(msg.Equals(msgCopy) ?
            "Объекты идентичны" : "Объекты различны");
        }
        public static void Task2()
        {
            var server = new Server();
            server.WaitForMessage();
        }
    }
}