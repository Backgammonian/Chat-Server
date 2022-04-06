using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Shared;

namespace ChatServer
{
    public class Client
    {
        private readonly Socket _client;
        private readonly Task _listenTask;
        private readonly CancellationTokenSource _tokenSource;

        public Client(Socket client)
        {
            _client = client;
            var size = Marshal.SizeOf((uint)0);
            var keepAlive = new byte[size * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(TcpKeepAliveConstants.TurnKeepAliveOn), 0, keepAlive, 0, size);
            Buffer.BlockCopy(BitConverter.GetBytes(TcpKeepAliveConstants.TimeWithoutActivity), 0, keepAlive, size, size);
            Buffer.BlockCopy(BitConverter.GetBytes(TcpKeepAliveConstants.KeepAliveInterval), 0, keepAlive, size * 2, size);
            _client.IOControl(IOControlCode.KeepAliveValues, keepAlive, null);

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            _listenTask = new Task(async () => await Listen(token));

            ClientAddress = _client.RemoteEndPoint as IPEndPoint;
        }

        public event EventHandler ErrorOccured;
        public event EventHandler<NetworkDataReceivedEventArgs> DataReceived;

        public IPEndPoint ClientAddress { get; }

        public async Task Send(byte[] data)
        {
            try
            {
                var dataLengthArray = BitConverter.GetBytes(data.Length);

                await _client.SendAsync(dataLengthArray, SocketFlags.None);
                await _client.SendAsync(data, SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ErrorOccured?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task<byte[]> Receive()
        {
            try
            {
                var dataLengthArray = new byte[4];
                await _client.ReceiveAsync(dataLengthArray, SocketFlags.None);

                var data = new byte[BitConverter.ToInt32(dataLengthArray)];
                await _client.ReceiveAsync(data, SocketFlags.None);

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _tokenSource.Cancel();

                ErrorOccured?.Invoke(this, EventArgs.Empty);

                return Array.Empty<byte>();
            }
        }

        private async Task Listen(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var data = await Receive();

                DataReceived?.Invoke(this, new NetworkDataReceivedEventArgs(data));
            }
        }

        public void StartListen()
        {
            _listenTask.Start();
        }
    }
}
