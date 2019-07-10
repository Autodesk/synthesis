using System.Collections.Generic;
using System.Threading;

public enum ThreadCommand
{
    KILL,
    RESET,
    IDLE,
    CONTINUE
}
public class Channel<T>
{
    private static readonly int TIMEOUT = 5; // milliseconds
    private System.Threading.Mutex mutex;
    private Queue<T> queue;
    private EventWaitHandle eventWaitHandle;

    public void Put(T item)
    {
        lock (mutex)
        {
            queue.Enqueue(item);
            eventWaitHandle.Set();
        }
    }

    public T Get()
    {
        eventWaitHandle.WaitOne();
        lock (mutex)
        {
            return queue.Dequeue();
        }
    }

    public Maybe<T> TryGet()
    {
        if (eventWaitHandle.WaitOne(TIMEOUT))
        {
            lock (mutex)
            {
                return new Maybe<T>(queue.Dequeue());
            }
        }
        return new Maybe<T>();
    }

    public Channel()
    {
        this.queue = new Queue<T>();
        this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
    }
}
