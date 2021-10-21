using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
                var client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...");

                var stream = client.GetStream();

                var buffer = new byte[2048];

                var rdCnt = stream.Read(buffer);

                var json = Encoding.UTF8.GetString(buffer, 0, rdCnt);

                Console.WriteLine("Client says: " + json);
                //Console.WriteLine($"Read count was {rdCnt}");

                var payload = JsonSerializer.Deserialize<Payload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Second parameter ensures that it is case insensitive
                
                var response = new Response("Missing method", "Test");
                var jsonResponse = JsonSerializer.Serialize(response);
                var responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                stream.Write(responseBytes);
            }
        }
    }

    class Payload
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }
    }

    class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }

        public Response(string StatusText, string BodyText)
        {
            Status = StatusText;
            Body = BodyText;
        }
    }
}
