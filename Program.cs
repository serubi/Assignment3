using System;
using System.Net;
using System.Net.Sockets;

namespace Assignment3
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine("Server has started...");

            while(true)
            {
                server.AcceptTcpClient();
                Console.WriteLine("Client connected...");
            }
        }
    }
}
