using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CLIServer
{
    internal class Program
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> Clients = new Dictionary<int, TcpClient>();

        static void Main(string[] args)
        {
            int count = 1;

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 5000);
            ServerSocket.Start();

            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) Clients.Add(count, client);
                Console.WriteLine($"{client.Client.RemoteEndPoint} connected to the server.");

                Thread t = new Thread(HandleClients);
                t.Start(count);
                count++;
            }
        }

        public static void HandleClients(object _object)
        {
            int id = (int)_object;
            TcpClient client;

            lock (_lock) client = Clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                Broadcasr(data);
                Console.WriteLine(data);
            }

            lock (_lock) Clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void Broadcasr(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in Clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
