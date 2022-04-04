using System.Collections.Generic;

namespace ChatServer
{
    public class Rooms
    {
        private readonly Dictionary<string, Room> _rooms;

        public Rooms()
        {
            _rooms = new Dictionary<string, Room>();
        }

        public Room this[string roomID]
        {
            get => _rooms[roomID];
            private set => _rooms[roomID] = value;
        }

        public bool Has(string roomID)
        {
            return _rooms.ContainsKey(roomID);
        }

        public bool Has(Room room)
        {
            return _rooms.ContainsKey(room.ID);
        }

        public void Add(Room room)
        {
            if (!Has(room))
            {
                _rooms.Add(room.ID, room);
            }
        }

        public void Remove(Room room)
        {
            if (Has(room))
            {
                _rooms.Remove(room.ID);
            }
        }

        public List<(string, string)> GetListOfRooms()
        {
            var result = new List<(string, string)>();

            foreach (var room in _rooms.Values)
            {
                result.Add((room.ID, room.Name));
            }

            return result;
        }
    }
}
