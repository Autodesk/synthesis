using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynthesisServer
{
    public class Lobby
    {
        // TODO: CATCH INDEX OUT OF BOUNDS
        //public string? Password { get; set; }
        public ClientData Host { get; private set; }
        public ClientData?[] Clients { get { return _clients; } }

        private readonly ClientData?[] _clients;

        public Lobby(ClientData host, int lobbySize)
        {
            _clients = new ClientData[lobbySize];

            _clients[0] = host;
            Host = host;
        } 

        public bool Swap(int firstIndex, int secondIndex)
        {
            try
            {
                var x = _clients[firstIndex];
                _clients[firstIndex] = _clients[secondIndex];
                _clients[secondIndex] = x;
                return true;
            }
            catch (IndexOutOfRangeException e)
            {
                return false;
            }
        }

        public bool TryAddClient(ClientData client)
        {
            // If no index is specified, it will try to add the client to an empty index so long as it does not already have a spot

            for (int i = 0; i < _clients.Length; i++)
            {
                if (client.Equals(_clients[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i] == null)
                {
                    _clients[i] = client;
                    return true;
                }
            }
            return false;
        }


        public bool TryRemoveClient(int index)
        {
            if (_clients[index] != null)
            {
                if (Host.Equals(_clients[index]))
                {
                    TryFindNewHost();
                }
                _clients[index] = null;
                return true;
            }
            return false;
        }

        public bool TryRemoveClient(ClientData client)
        {
            if (client.Equals(Host)) {
                TryFindNewHost();
            }
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Equals(client))
                {
                    _clients[i] = null;
                    return true;
                }
            }
            return false;
        }

        private bool TryFindNewHost()
        {
            foreach (ClientData x in _clients)
            {
                if (x != null && !x.Equals(Host))
                {
                    Host = x;
                    return true;
                }
            }
            return false;
        }
    }
}
