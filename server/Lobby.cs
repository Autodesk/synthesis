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
        public bool ShouldDie { get; private set; }

        private ReaderWriterLockSlim _clientsLock;
        private readonly ClientData[] _clients;

        public Lobby(ClientData host)
        {
            ShouldDie = false;

            _clients = new ClientData[6];
            _clientsLock = new ReaderWriterLockSlim();

            _clients[0] = host;
            Host = host;
        } 

        public void Swap(int firstIndex, int secondIndex)
        {
            _clientsLock.EnterWriteLock();
            var x = _clients[firstIndex];
            _clients[firstIndex] = _clients[secondIndex];
            _clients[secondIndex] = x;
            _clientsLock.ExitWriteLock();
        }

        public bool TrySetClient(ClientData client, int index)
        {
            _clientsLock.EnterWriteLock();
            if (_clients[index] == null)
            {
                for (int i = 0; i < _clients.Length; i++)
                {
                    if (client.Equals(_clients[i]))
                    {
                        _clients[i] = null;
                    }
                }
                _clients[index] = client;
                return true;
            }
            return false;
        }

        public bool TryAddClient(ClientData client)
        {
            _clientsLock.EnterWriteLock();
            for (int i = 0; i < _clients.Length; i++)
            {
                if (client.Equals(_clients[i]))
                {
                    _clientsLock.ExitWriteLock();
                    return false;
                }
            }
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i] == null)
                {
                    _clients[i] = client;
                    _clientsLock.ExitWriteLock();
                    return true;
                }
            }
            _clientsLock.ExitWriteLock();
            return false;
        }

        public bool TryRemoveClient(int index)
        {
            _clientsLock.EnterWriteLock();
            if (_clients[index] != null)
            {
                if (_clients[index].Equals(Host) && !TryFindNewHost())
                {
                    ShouldDie = true;
                }
                _clients[index] = null;
                _clientsLock.ExitWriteLock();
                return true;
            }
            _clientsLock.ExitWriteLock();
            return false;
        }

        public bool TryRemoveClient(ClientData client)
        {
            _clientsLock.EnterWriteLock();
            if (client.Equals(Host) && !TryFindNewHost())
            {
                ShouldDie = true;
            }
            for (int i = 0; i < _clients.Length; i++)
            {
                if (_clients[i].Equals(client))
                {
                    _clients[i] = null;
                    _clientsLock.ExitWriteLock();
                    return true;
                }
            }
            _clientsLock.ExitWriteLock();
            return false;
        }

        private bool TryFindNewHost()
        {
            _clientsLock.EnterReadLock();
            foreach (ClientData x in _clients)
            {
                if (x != null && !x.Equals(Host))
                {
                    Host = x;
                    return true;
                }
            }
            _clientsLock.ExitReadLock();
            return false;
        }
    }
}
