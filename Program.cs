using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Assignment3
{
    class Program
    {
        delegate void Transformer(string x);

        static void Main(string[] args)
        {
            //var validMethods = ["create", "read", "update", "delete", "echo"];
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine("Server has started...");

            var buffer = new byte[1024];

            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...");

                try { 
                    var stream = client.GetStream();
                    var rdCnt = stream.Read(buffer);
                    var json = Encoding.UTF8.GetString(buffer, 0, rdCnt);

                    Console.WriteLine("Client payload: " + json);

                    var payload = JsonSerializer.Deserialize<Payload>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Second parameter ensures that it is case insensitive

                    var response = new Response();
                    response.Status = "";

                    // Check payload for valid Method
                    if (payload.Method == null)
                    {
                        Console.WriteLine("Missing method");
                        response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing method";
                    } else
                    {
                        var protocolMethod = new Transformer(Create);

                        switch (payload.Method.ToLower())
                        {
                            case "create":
                                protocolMethod = new Transformer(Create);
                                break;
                            default:
                                Console.WriteLine("Illegal method");
                                response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal method";
                                break;
                        }
                    }

                    // Check payload for valid Path
                    if (payload.Path == null)
                    {
                        Console.WriteLine("Missing resource");
                        response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing resource";
                    }
                    else
                    {
                        switch (payload.Path.ToLower())
                        {
                            case "test":

                                break;
                            default:
                                Console.WriteLine("Illegal resource");
                                response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal resource";
                                break;
                        }
                    }

                    // Check payload for valid Date
                    if (payload.Date == null)
                    {
                        Console.WriteLine("Missing date");
                        response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing date";
                    }
                    else if (!int.TryParse(payload.Date, out int timestamp))
                    {
                        Console.WriteLine("Illegal date");
                        response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal date";
                    }

                    var jsonResponse = JsonSerializer.Serialize(response);
                    Console.WriteLine(jsonResponse);
                    var responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                    stream.Write(responseBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
            }
        }

        static void Create(string Path)
        {
            Console.WriteLine("Creating item");
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

        //public Response(string StatusText, string BodyText)
        //{
        //    Status = StatusText;
        //    Body = BodyText;
        //}
    }
}
