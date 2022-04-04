using System.Collections.Generic;

namespace ChatServer
{
    public class ClientProfiles
    {
        private readonly Dictionary<string, ClientProfile> _clients;

        public ClientProfiles()
        {
            _clients = new Dictionary<string, ClientProfile>();
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

                return true;
            }

            return false;
        }

        public bool Remove(ClientProfile client)
        {
            if (Has(client))
            {
                _clients.Remove(client.ID);

                return true;
            }

            return false;
        }
    }
}
