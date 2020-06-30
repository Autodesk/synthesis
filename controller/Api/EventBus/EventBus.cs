using System;
using System.Collections.Generic;

namespace SynthesisAPI.EventBus
{
    public static class EventBus
    {
        public delegate void EventCallback(IEvent eventInfo);

        /// <summary>
        /// Activates all callback functions that are listening to a particular
        /// EventType and passes event information to those functions 
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="eventInfo">Event to be passed to callback functions</param>
        public static void Push<TEvent>(TEvent eventInfo) where TEvent : IEvent
        {
            if (Instance.typeSubscribers.ContainsKey(eventInfo.EventType))
            {
                Instance.typeSubscribers[eventInfo.EventType](eventInfo);
            }
            else
                throw new Exception(message: "No subscribers found");
        }

        /// <summary>
        /// Activates all callback functions that are listening to a particular
        /// tag and passes event information to those functions 
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="eventInfo">Event to be passed to callback functions</param>
        /// <param name="tag">Tag corresponding to the group of listener functions to be activated</param>
       
        public static void Push<TEvent>(string tag, TEvent eventInfo) where TEvent : IEvent
        {
            if (Instance.tagSubscribers.ContainsKey(tag))
            {
                Instance.tagSubscribers[tag](eventInfo);
                if (Instance.typeSubscribers.ContainsKey(eventInfo.EventType))
                {
                    Instance.typeSubscribers[eventInfo.EventType](eventInfo);
                }
            }
            else if (Instance.typeSubscribers.ContainsKey(eventInfo.EventType))
            {
                Instance.typeSubscribers[eventInfo.EventType](eventInfo);
            }
            else
                throw new Exception(message: "No subscribers found");
        }

        /// <summary>
        /// Adds a callback function that is activated by an event of
        /// the specified type
        /// </summary>
        /// <param name="type">The event type to listen for</param>
        /// <param name="callback">The callback function to be activated</param>
        public static void NewTypeListener(string type, EventCallback callback)
        {
            if (Instance.typeSubscribers.ContainsKey(type))
                Instance.typeSubscribers[type] += callback;
            else
                Instance.typeSubscribers.Add(type, callback);
        }

        /// <summary>
        /// Adds a callback function that is activated by an event 
        /// being pushed to the specified tag
        /// </summary>
        /// <param name="tag">The tag to listen for</param>
        /// <param name="callback">The callback function to be activated</param>
        public static void NewTagListener(string tag, EventCallback callback)
        {
            if (Instance.tagSubscribers.ContainsKey(tag))
                Instance.tagSubscribers[tag] += callback;
            else
                Instance.tagSubscribers.Add(tag, callback);
        }

        public static void resetAllListeners()
        {
            Instance.typeSubscribers = new Dictionary<string, EventCallback>();
            Instance.tagSubscribers = new Dictionary<string, EventCallback>();
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

/*   public class Subscriber
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