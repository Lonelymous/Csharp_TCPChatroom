using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CLIClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 5000;

            TcpClient client = new TcpClient();
            client.Connect(ip, port);
            
            Console.WriteLine($"Connected to the server ({ip}:{port})");
            
            NetworkStream ns = client.GetStream();
            Thread thread = new Thread(_object => ReceiveData((TcpClient)_object));

            thread.Start(client);

            string s;
            while (!string.IsNullOrEmpty(s = Console.ReadLine()))
            {
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
            }

            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.WriteLine("Disconnected from the server!");
            Console.ReadKey();
        }

        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                Console.Write(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
            }
        }
    }
}
