using System;
using System.Collections.Generic;
using System.Threading;

namespace Synthesis
{
    public class Channel<T>
    {
        private bool isOpen;
        private const int TIMEOUT = 5; // milliseconds
        private Mutex mutex;
        private Queue<T> buffer;
        private EventWaitHandle eventWaitHandle;

        public Channel()
        {
            mutex = new Mutex();
            buffer = new Queue<T>();
            eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            isOpen = true;
        }

        public void Send(T item)
        {
            if (!isOpen)
            {
                throw new Exception("Attempting push to closed channel");
            }
            lock (mutex)
            {
                buffer.Enqueue(item);
                eventWaitHandle.Set();
            }
        }

        public void Pop()
        {
            lock (mutex)
            {
                buffer.Dequeue();
            }
        }

        public T Get()
        {
            eventWaitHandle.WaitOne();
            lock (mutex)
            {
                return buffer.Dequeue();
            }
        }

        public Optional<T> TryGet()
        {
            if (eventWaitHandle.WaitOne(TIMEOUT))
            {
                lock (mutex)
                {
                    return buffer.Count != 0 ? new Optional<T>(buffer.Dequeue()) : new Optional<T>();
                }
            }
            return new Optional<T>();
        }

        public T Peek()
        {
            eventWaitHandle.WaitOne();
            lock (mutex)
            {
                return buffer.Peek();
            }
        }

        public Optional<T> TryPeek()
        {
            if (eventWaitHandle.WaitOne(TIMEOUT))
            {
                lock (mutex)
                {
                    return buffer.Count != 0 ? new Optional<T>(buffer.Peek()) : new Optional<T>();
                }
            }
            return new Optional<T>();
        }


        public void Close()
        {
            isOpen = false;
        }

        public static Tuple<Channel<T>, Channel<R>> CreateOneshot<R>()
        {
            return new Tuple<Channel<T>, Channel<R>>(new Channel<T>(), new Channel<R>());
        }
    }
}