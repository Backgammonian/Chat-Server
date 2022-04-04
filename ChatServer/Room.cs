using System;
using System.Collections.Generic;
using ChatMessages;
using Shared;

namespace ChatServer
{
    public class Room
    {
        private readonly List<Message> _messages;

        public event EventHandler NewMessageArrived;

        public Room(string name)
        {
            _messages = new List<Message>();
            ID = RandomGenerator.GetRandomString(20);
            Name = name;
        }

        public string ID { get; }
        public string Name { get; }

        public void AddNewMessage(Message message)
        {
            _messages.Add(message);

            NewMessageArrived?.Invoke(this, EventArgs.Empty);
        }

        public List<Message> GetMessages()
        {
            return _messages;
        }
    }
}
