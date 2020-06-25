using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections;
using SynthesisAPI.EventBus;

namespace SynthesisAPI.EventBus
{
    public static class EventBus
    {
        public static Dictionary<string, Channel<IEvent>> topics = new Dictionary<string, Channel<IEvent>>();

        static EventBus()
        {

        }

        private static Channel<IEvent> CreateChannel(string tag)
        {
            Channel<IEvent> c = Channel.CreateUnbounded<IEvent>();
            topics.Add(tag, c);
            return c;
        }
        public static void Push(IEvent event, String [] tags)
        {
            foreach (string tag in tags)
            {
                Channel<IEvent> ch;
                if (topics.TryGetValue(tag, out ch)) 
                {
                    await ch.Writer.WriteAsync(event);
                    ch.Writer.Complete();
                } 
                else 
                {
                    ch = CreateChannel(tag);
                    await ch.Writer.WriteAsync(event);
                    ch.Writer.Complete();
                }
            }
        }

        public static ChannelReader<IEvent> Subscribe (string tag)
        {
            Channel<IEvent> ch;
            if (topics.TryGetValue(tag, out ch))
            {
                return ch.Reader;
            }
            else
            {
                ch = CreateChannel(tag);
                return ch.Reader;
            }
        }

    }
}
