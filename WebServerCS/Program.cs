using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using WebServerCS;

namespace Webserver_csharp
{
    class Program
    {
        private static TcpListener myListener;
        private static int port = 5051;
        private static IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        private static string WWWWebServerPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName + "\\www";
        private static string serverEtag = Guid.NewGuid().ToString("N");

        static void Main(string[] args)
        {
            try
            {
                myListener = new TcpListener(localAddr, port);
                myListener.Start();
                Console.WriteLine($"Web Server Running on {localAddr} on port {port}... Press ^C to Stop...");
                Thread th = new Thread(new ThreadStart(StartListen));
                th.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void StartListen()
        {
            while (true)
            {
                TcpClient client = myListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                // Read request 
                byte[] requestBytes = new byte[1024];
                int bytesRead = stream.Read(requestBytes, 0, requestBytes.Length);

                string request = Encoding.UTF8.GetString(requestBytes, 0, bytesRead);
                Console.WriteLine("--------------------------- REQUEST  ------------------------------------");
                Console.WriteLine(request);
                var requestHeaders = ParseHeaders(request);

                string[] requestFirstLine = requestHeaders.requestType.Split(" ");
                string httpVersion = requestFirstLine.LastOrDefault();
                string contentType = requestHeaders.headers.GetValueOrDefault("Accept");
                string contentEncoding = requestHeaders.headers.GetValueOrDefault("Accept-Encoding");

                if (request.StartsWith("GET"))
                {
                    var req = requestFirstLine[1];
                    string[] requestLine = req.Split("?");
                    var requestedPath = requestLine[0];

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    if (requestLine.Length > 1)
                    {
                        string[] paramPairs = requestLine[1].Split("&");

                        foreach (string pair in paramPairs)
                        {
                            string[] detail = pair.Split("=");
                            string name = detail[0];
                            string value = detail[1];
                            parameters[name] = value;
                        }
                    }

                    if (requestedPath == "/sela")
                    {
                        // Utilize o Adapter para traduzir os parâmetros e resolver o sistema
                        var adapter = new LinearSystemAdapter(parameters);
                        var (solution, algorithmName) = adapter.Solve();

                        // Crie a resposta em HTML
                        string responseBody = $"<html><body><h1>Resultado do Sistema Linear ({algorithmName})</h1><p>{solution}</p></body></html>";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

                        string header = SendHeaders(httpVersion, 200, "OK", "text/html; charset=UTF-8", null, responseBytes.Length, ref stream);
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine("--------------------------- RESPONSE  ------------------------------------");
                        Console.WriteLine(header + responseBody);
                    }
                    else
                    {
                        var fileContent = GetContent(requestedPath);
                        if (fileContent is not null)
                        {
                            string header = SendHeaders(httpVersion, 200, "OK", "text/html; charset=UTF-8", null, fileContent.Length, ref stream);
                            stream.Write(fileContent, 0, fileContent.Length);
                            Console.WriteLine("--------------------------- RESPONSE  ------------------------------------");
                            Console.WriteLine(header + Encoding.UTF8.GetString(fileContent));
                        }
                        else
                        {
                            string header = SendHeaders(httpVersion, 404, "Page Not Found", "text/html; charset=UTF-8", null, 0, ref stream);
                            Console.WriteLine("--------------------------- RESPONSE  ------------------------------------");
                            Console.WriteLine(header);
                        }
                    }
                }
                else
                {
                    string header = SendHeaders(httpVersion, 405, "Method Not Allowed", "text/html; charset=UTF-8", null, 0, ref stream);
                    Console.WriteLine(header);
                }

                client.Close();
            }
        }

        private static byte[] GetContent(string requestedPath)
        {
            if (requestedPath == "/") requestedPath = "default.html";
            if (requestedPath == "/padrao") requestedPath = "padrao.html";
            if (requestedPath == "/exemplo_GET") requestedPath = "exemplo_GET.html";
            if (requestedPath == "/exemplo_POST") requestedPath = "exemplo_POST.html";

            string filePath = Path.Join(WWWWebServerPath, requestedPath);

            if (!File.Exists(filePath)) return null;

            else
            {
                byte[] file = File.ReadAllBytes(filePath);
                return file;
            }
        }

        private static string SendHeaders(string? httpVersion, int statusCode, string statusMsg, string? contentType, string? contentEncoding, int byteLength, ref NetworkStream networkStream)
        {
            string responseHeaderBuffer = $"HTTP/1.1 {statusCode} {statusMsg}\r\n" +
                                          "Connection: close\r\n" +
                                          $"Date: {DateTime.UtcNow}\r\n" +
                                          $"Server: SimpleCSharpServer\r\n" +
                                          $"Etag: \"{serverEtag}\"\r\n" +
                                          $"Content-Type: {contentType}\r\n" +
                                          $"Content-Length: {byteLength}\r\n" +
                                          "\r\n";

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseHeaderBuffer);
            networkStream.Write(responseBytes, 0, responseBytes.Length);

            return Encoding.UTF8.GetString(responseBytes);
        }

        private static (Dictionary<string, string> headers, string requestType) ParseHeaders(string headerString)
        {
            var headerLines = headerString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string firstLine = headerLines[0];
            var headerValues = new Dictionary<string, string>();
            foreach (var headerLine in headerLines.Skip(1))
            {
                var headerDetail = headerLine.Trim();
                var delimiterIndex = headerDetail.IndexOf(':');
                if (delimiterIndex >= 0)
                {
                    var headerName = headerDetail.Substring(0, delimiterIndex).Trim();
                    var headerValue = headerDetail.Substring(delimiterIndex + 1).Trim();
                    headerValues.Add(headerName, headerValue);
                }
            }
            return (headerValues, firstLine);
        }
    }
}
