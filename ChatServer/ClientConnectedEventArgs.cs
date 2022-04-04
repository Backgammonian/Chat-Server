using System;
using System.Net.Sockets;

namespace ChatServer
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnectedEventArgs(Socket client)
        {
            Client = client;
        }

        public Socket Client { get; }
    }
}
