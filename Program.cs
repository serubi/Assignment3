using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assignment3
{
    class Program
    {
        delegate void Transformer(string x);

        static void Main(string[] args)
        {
            var categories = new List<Category>
            {
                new Category{CID = 1, Name = "Beverages"},
                new Category{CID = 2, Name = "Condiments"},
                new Category{CID = 3, Name = "Confections"}
            };

            //var validMethods = ["create", "read", "update", "delete", "echo"];
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine("Server has started...");

            var buffer = new byte[2048];

            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("Client connected...");

                try { 
                    var stream = client.GetStream();
                    var rdCnt = stream.Read(buffer);
                    var json = Encoding.UTF8.GetString(buffer, 0, rdCnt);

                    Console.WriteLine("Client payload: " + json);

                    var payload = JsonSerializer.Deserialize<Payload>(json); // Second parameter ensures that it is case insensitive

                    var response = new Response();
                    response.Status = "";

                    // Check payload for valid Method
                    if (payload.Method == null)
                    {
                        Console.WriteLine("Missing method");
                        response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing method";
                        payload.Method = "";
                    } else
                    {
                        var protocolMethod = new Transformer(Create);

                        switch (payload.Method.ToLower())
                        {
                            case "create":
                                protocolMethod = new Transformer(Create);
                                break;
                            case "read":
                                protocolMethod = new Transformer(Create);
                                break;
                            case "update":
                                protocolMethod = new Transformer(Create);
                                break;
                            case "delete":
                                protocolMethod = new Transformer(Create);
                                break;
                            case "echo":
                                protocolMethod = new Transformer(Create);
                                break;
                            default:
                                Console.WriteLine("Illegal method");
                                response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal method";
                                break;
                        }
                    }

                    // Check payload for valid Path
                    if (payload.Method.ToLower() != "echo")
                    {
                        if (payload.Path == null)
                        {
                            Console.WriteLine("Missing resource");
                            response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing resource";
                        }
                        else
                        {
                            var pathArray = payload.Path.Split('/'); // first index will be empty, because the path starts with /
                            if (pathArray.Length < 2)
                            {
                                Console.WriteLine("Illegal resource");
                                response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal resource";
                            } else if (pathArray[1] != "api" || pathArray[2] != "categories" || (pathArray.Length >= 4 && !int.TryParse(pathArray[3], out int CID)) || (pathArray.Length >= 4 && payload.Method.ToLower() == "create") || (pathArray.Length <= 3 && (payload.Method.ToLower() == "update" || payload.Method.ToLower() == "delete")))
                            {
                                Console.WriteLine("Bad Request");
                                response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Bad Request";
                            }
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

                    // Check payload for valid Body, except if method is read or delete
                    if (payload.Method.ToLower() != "read" && payload.Method.ToLower() != "delete")
                    {
                        if (payload.Body == null)
                        {
                            Console.WriteLine("Missing body");
                            response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Missing body";
                        }
                        else
                        {
                            if (payload.Method.ToLower() != "echo")
                            {
                                // Validate that the payload body is json
                                try
                                {
                                    JsonDocument.Parse(payload.Body);
                                }
                                catch (JsonException)
                                {
                                    Console.WriteLine("Illegal body");
                                    response.Status += (response.Status.Length == 0 ? "4 " : ", ") + "Illegal body";
                                }
                            }
                        }
                    }

                    if (response.Status == "")
                    {
                        var pathArray = payload.Path.Split('/'); // first index will be empty, because the path starts with /

                        // No previous status was supplied, so assume everything is good to proceed
                        if (payload.Method.ToLower() == "echo")
                        {
                            response.Status = "1 Ok";
                            response.Body = payload.Body;
                        }
                        else if (payload.Method.ToLower() == "read")
                        {
                            if (pathArray.Length >= 4)
                            {
                                // Return individual category, since cid is provided
                                foreach (var category in categories)
                                {
                                    if (category.CID == int.Parse(pathArray[3]))
                                    {
                                        response.Status = "1 Ok";
                                        response.Body = JsonSerializer.Serialize(category);
                                        break;
                                    }
                                }

                                if (response.Status == "")
                                {
                                    // No category with that id was found
                                    response.Status = "5 Not found";
                                }
                            } else
                            {
                                response.Status = "1 Ok";
                                // Return all categories, since no cid is provided
                                response.Body = JsonSerializer.Serialize(categories);
                            }
                        }
                        else if (payload.Method.ToLower() == "update")
                        {
                            if (pathArray.Length >= 4)
                            {
                                // Return individual category, since cid is provided
                                foreach (var category in categories)
                                {
                                    if (category.CID == int.Parse(pathArray[3]))
                                    {
                                        var updatedCategory = JsonSerializer.Deserialize<Category>(payload.Body);
                                        category.Name = updatedCategory.Name;
                                        response.Status = "3 Updated";
                                        response.Body = JsonSerializer.Serialize(category);
                                        break;
                                    }
                                }

                                if (response.Status == "")
                                {
                                    // No category with that id was found
                                    response.Status = "5 Not found";
                                }
                            }
                            else
                            {
                                //response.Status = "1 Ok";
                                //// Return all categories, since no cid is provided
                                //response.Body = JsonSerializer.Serialize(categories);
                            }
                        }
                    }

                    var jsonResponse = JsonSerializer.Serialize(response);
                    Console.WriteLine("Server response: " + jsonResponse);
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
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }

    class Response
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }

        //public Response(string StatusText, string BodyText)
        //{
        //    Status = StatusText;
        //    Body = BodyText;
        //}
    }

    class Category
    {
        [JsonPropertyName("cid")]
        public int CID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
