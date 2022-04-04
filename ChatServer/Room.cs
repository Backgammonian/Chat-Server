using System;
using System.Collections.Generic;
using ChatMessages;
using Shared;

namespace ChatServer
{
    public class Room
    {
        private readonly List<Message> _messages;

        public Room(string name)
        {
            _messages = new List<Message>();
            ID = RandomGenerator.GetRandomString(20);
            Name = name;
        }

        public event EventHandler<NewMessageInRoomEventArgs> NewMessageArrived;

        public string ID { get; }
        public string Name { get; }

        public void AddNewMessage(Message message)
        {
            _messages.Add(message);

            NewMessageArrived?.Invoke(this, new NewMessageInRoomEventArgs(ID, message));
        }

        public List<Message> GetMessages()
        {
            return _messages;
        }
    }
}
