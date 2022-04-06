using System;
using System.Threading.Tasks;
using Shared;

namespace ChatServer
{
    public class ClientProfile
    {
        private readonly Client _client;
        private string _nickname;
        private Room _currentRoom;

        public ClientProfile(Client client)
        {
            _client = client;
            _client.DataReceived += OnDataReceived;
            _client.ErrorOccured += OnErrorOccured;
            _client.StartListen();
            ID = _client.ClientAddress.ToString();
            Nickname = "UserName";
            IsLoaded = false;
        }

        public event EventHandler ClientLoaded;
        public event EventHandler ErrorOccured;
        public event EventHandler<NetworkDataReceivedEventArgs> DataReceived;
        public event EventHandler<NewMessageInRoomEventArgs> NewMessageInRoomReceived;

        public string ID { get; }
        public string LocalID { get; private set; }
        public bool IsLoaded { get; private set; }

        public string Nickname 
        {
            get => _nickname;
            private set
            {
                if (!string.IsNullOrEmpty(value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    _nickname = value;
                }
            }
        }

        public async Task Send(byte[] data)
        {
            await _client.Send(data);
        }

        private void OnDataReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public void SetInfo(string localID, string nickname)
        {
            if (!IsLoaded)
            {
                LocalID = localID;
                Nickname = nickname;
            }

            IsLoaded = true;
            ClientLoaded?.Invoke(this, EventArgs.Empty);
        }

        public void SetNickname(string nickname)
        {
            Nickname = nickname;
        }

        public void SetRoom(Room room)
        {
            if (_currentRoom != null)
            {
                _currentRoom.NewMessageArrived -= OnReceiveMessageFromRoom;
            }

            _currentRoom = room;
            _currentRoom.NewMessageArrived += OnReceiveMessageFromRoom;
        }

        private void OnReceiveMessageFromRoom(object sender, NewMessageInRoomEventArgs e)
        {
            NewMessageInRoomReceived?.Invoke(this, e);
        }

        private void OnErrorOccured(object sender, EventArgs e)
        {
            ErrorOccured?.Invoke(this, EventArgs.Empty);
        }
    }
}
