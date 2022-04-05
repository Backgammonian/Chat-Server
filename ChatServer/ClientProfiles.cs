using System;
using System.Collections;
using System.Collections.Generic;

namespace ChatServer
{
    public class ClientProfiles : IEnumerable<ClientProfile>
    {
        private readonly Dictionary<string, ClientProfile> _clients;

        public ClientProfiles()
        {
            _clients = new Dictionary<string, ClientProfile>();
        }

        public event EventHandler ClientsListUpdated;

        public ClientProfile this[string clientID]
        {
            get => _clients[clientID];
            private set => _clients[clientID] = value;
        }

        public bool Has(ClientProfile client)
        {
            return _clients.ContainsKey(client.ID);
        }

        public bool Add(ClientProfile client)
        {
            if (!Has(client))
            {
                _clients.Add(client.ID, client);
                ClientsListUpdated?.Invoke(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        public bool Remove(ClientProfile client)
        {
            if (Has(client))
            {
                _clients.Remove(client.ID);
                ClientsListUpdated?.Invoke(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        public List<(string, string, string)> GetListOfClients()
        {
            var result = new List<(string, string, string)>();

            foreach (var client in _clients.Values)
            {
                result.Add((client.Nickname, client.LocalID, client.ID));
            }

            return result;
        }

        public IEnumerator<ClientProfile> GetEnumerator()
        {
            return _clients.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_clients.Values).GetEnumerator();
        }
    }
}
