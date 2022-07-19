using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisServer
{
    class BufferManager
    {
        private int _totalBytes;
        private byte[] _buffer;
        
        private int _currentIndex;
        private int _subBufferSize;

        private Stack<int> _freeIndexPool;
        private readonly Dictionary<string, int> _registeredBuffers;
        
        public BufferManager(int totalBytes, int bufferSize)
        {
            _totalBytes = totalBytes;
            _currentIndex = 0;
            _subBufferSize = bufferSize;

            _freeIndexPool = new Stack<int>();
            _registeredBuffers = new Dictionary<string, int>();
        }

        public void Init()
        {
            _buffer = new byte[_totalBytes];
        }

        public bool TryRegisterBuffer(string key)
        {
            if (_registeredBuffers.ContainsKey(key)) { return false; }
            if (_freeIndexPool.Count > 0)
            {
                _registeredBuffers[key] = _freeIndexPool.Pop();
            }
            else
            {
                if (_currentIndex > _totalBytes - _subBufferSize) { return false; }

                _registeredBuffers[key] = _currentIndex;
                _currentIndex += _subBufferSize;
            }
            return true;
        }

        public bool FreeBuffer(string key)
        {
            if (!_registeredBuffers.ContainsKey(key)) { return false; }
            _freeIndexPool.Push(_registeredBuffers[key]);
            byte[] empty = new byte[_subBufferSize];
            Array.Copy(empty, 0, _buffer, _registeredBuffers[key], _subBufferSize);
            _registeredBuffers.Remove(key);
            return true;
        }

        public byte[] GetBaseBuffer() { return _buffer; }
        public int GetBufferLength() { return _subBufferSize; }
        public int GetRemainingBufferCount() { return ((_totalBytes - _currentIndex) / _subBufferSize) + _freeIndexPool.Count; }

        // return -1 if no buffer is registered for the key specified
        public int GetOffset(string key)
        {
            if (_registeredBuffers.ContainsKey(key))
            {
                return _registeredBuffers[key];
            }
            else
            {
                return -1;
            }
        }
    }
}
