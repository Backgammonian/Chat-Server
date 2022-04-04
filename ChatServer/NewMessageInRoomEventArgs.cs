using System;
using ChatMessages;

namespace ChatServer
{
    public class NewMessageInRoomEventArgs : EventArgs
    {
        public NewMessageInRoomEventArgs(string roomID, Message message)
        {
            RoomID = roomID;
            Message = message;
        }

        public string RoomID { get; }
        public Message Message { get; }
    }
}
