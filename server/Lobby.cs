using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynthesisServer
{
    class Lobby
    {
        public string Name { get; set; }
        public string? Password { get; set; }
        public ClientData Host { get; set; }

        private ReaderWriterLockSlim _clientsLock;
        private readonly ClientData[] _clients;

        public Lobby()
        {
            _clients = new ClientData[6];
            _clientsLock = new ReaderWriterLockSlim();
        } 

        public void Swap(int firstIndex, int secondIndex)
        {
            _clientsLock.EnterWriteLock();
            var x = _clients[firstIndex];
            _clients[firstIndex] = _clients[secondIndex];
            _clients[secondIndex] = x;
            _clientsLock.ExitWriteLock();
        }

        public bool IsFull()
        {
            _clientsLock.EnterReadLock();
            foreach (var x in _clients)
            {
                if (x == null)
                {
                    _clientsLock.ExitReadLock();
                    return false;
                }
            }
            _clientsLock.ExitReadLock();
            return true;
        }

        public bool TryAddClient(ClientData client)
        {
            _clientsLock.EnterWriteLock();
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
                _clients[index] = null;
                return true;
            }
            return false;
        }
    }
}
