using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    public class Server
    {
        private readonly Socket _listener;
        private bool _isStarted;
        private bool _isRunning;

        public Server(ushort port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            NetInterface = new IPEndPoint(IPAddress.Any, port);
        }

        public event EventHandler<ClientConnectedEventArgs> ClientConnected;

        public IPEndPoint NetInterface { get; }

        public async Task StartListen()
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                _listener.Bind(NetInterface);
                _listener.Listen(65536);

                _isStarted = true;
                _isRunning = true;

                while (_isRunning)
                {
                    var incomingSocket = await _listener.AcceptAsync();
                    ClientConnected?.Invoke(this, new ClientConnectedEventArgs(incomingSocket));

                    Console.WriteLine(NetInterface + " got connection from " + incomingSocket.RemoteEndPoint);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                _isRunning = false;
            }
        }

        public void Stop()
        {
            _isRunning = false;

            Console.WriteLine("Server " + NetInterface + " is done listening");

            _listener.Close();
            _listener.Dispose();
        }
    }
}
