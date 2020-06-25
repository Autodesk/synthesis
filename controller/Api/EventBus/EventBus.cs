using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections;
using SynthesisAPI.EventBus;
using System.Diagnostics.Tracing;

namespace SynthesisAPI.EventBus
{
    public static class EventBus
    {
        public static Dictionary<string, Channel<IEvent>[]> topics = new Dictionary<string, Channel<IEvent>[]>();
        static EventBus()
        {

        }

        public static void Push(IEvent event, String [] tags)
        {
            foreach (string tag in tags)
            {
                Channel<IEvent>[] ch;
                if (topics.TryGetValue(tag, out ch)) 
                {
                    foreach(Channel<IEvent> channel in ch)
                    {
                        
                        await channel.Writer.WriteAsync(event);
                        channel.Writer.Complete();
                    }
                } 
                else 
                {
                    Channel<IEvent> channel = Channel.CreateUnbounded<IEvent>();
                    ch = new Channel<IEvent>[]{channel};
                    topics.Add(tag, ch);
                    await channel.Writer.WriteAsync(event);
                    channel.Writer.Complete();
                }
            }
        }

        public ChannelReader<IEvent> Subscribe(string tag)
        {
            Channel<IEvent>[] ch;
            Channel<IEvent> channel = Channel.CreateUnbounded<IEvent>();
            if (topics.TryGetValue(tag, out ch))
            {
                ch.Append(channel);
            } 
            else
            {
                ch = new Channel<IEvent>[]{channel};
                topics.Add(tag, ch);
            }

            return channel.Reader;
        }
        public static IEvent NewTagListener(string tag)
        {
            Channel<IEvent> ch;
            if (topics.TryGetValue(tag, out ch))
            {
                IEvent result; 
                ch.Reader.TryRead(out result);
                return result;
            }
        }

    }
}
