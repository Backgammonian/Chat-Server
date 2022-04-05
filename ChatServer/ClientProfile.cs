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
        private bool _isLocalIDSet;

        public ClientProfile(Client client)
        {
            _client = client;
            _client.DataReceived += OnDataReceived;
            _client.StartListen();
            ID = _client.ClientAddress.ToString();
            Nickname = "UserName";
        }

        public event EventHandler<NetworkDataReceivedEventArgs> DataReceived;
        public event EventHandler<NewMessageInRoomEventArgs> NewMessageInRoomReceived;

        public string ID { get; }
        public string LocalID { get; private set; }

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

        public void SetNickname(string nickname)
        {
            Nickname = nickname;
        }

        public void SetLocalID(string localID)
        {
            if (!_isLocalIDSet)
            {
                LocalID = localID;
            }
            
            _isLocalIDSet = true;
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
    }
}
