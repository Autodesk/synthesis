using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Collections;
using SynthesisAPI.EventBus;
using System.Diagnostics.Tracing;
using UnityEngine;

namespace SynthesisAPI.EventBus
{
    public static class EventBus
    {
        public delegate void EventCallback(IEvent eventInfo);

        public static void Push<TEvent>(TEvent eventInfo) where TEvent : IEvent
        {
            if (Instance.tagSubscribers.ContainsKey(eventInfo.EventType))
            {
                Instance.tagSubscribers[eventInfo.EventType](eventInfo);
            }
            else
                throw new Exception(message: "No subscribers found");
        }

        public static void Push<TEvent>(TEvent eventInfo, string tag) where TEvent : IEvent
        {
            if (Instance.tagSubscribers.ContainsKey(tag))
            {
                Instance.tagSubscribers[tag](eventInfo);
            }
            else
                throw new Exception(message: "No subscribers found");
        }

        public static void NewTypeListener(string type, EventCallback callback)
        {
            if (Instance.typeSubscribers.ContainsKey(type))
                Instance.typeSubscribers[type] += callback;
            else
                Instance.typeSubscribers.Add(type, callback);
        }

        public static void NewTagListener(string tag, EventCallback callback)
        {
            if (Instance.tagSubscribers.ContainsKey(tag))
                Instance.tagSubscribers[tag] += callback;
            else
                Instance.tagSubscribers.Add(tag, callback);
        }

        private class Inner
        {
            public Dictionary<string, EventCallback> typeSubscribers;
            public Dictionary<string, EventCallback> tagSubscribers;

            private Inner()
            {
                typeSubscribers = new Dictionary<string, EventCallback>();
                tagSubscribers = new Dictionary<string, EventCallback>();
            }

            private static Inner _inst;
            public static Inner InnerInstance
            {
                get
                {
                    if (_inst == null)
                        _inst = new Inner();
                    return _inst;
                }
            }
        }

        private static Inner Instance => Inner.InnerInstance;
    }

/*    public class Subscriber
    {
        public Subscriber()
        {
            EventBus.Sub("test", CallBackMethod);
        }

        public void CallBackMethod(IEvent e)
        {

        }
    }*/
}