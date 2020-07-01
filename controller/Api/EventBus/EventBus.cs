using System;
using System.Collections.Generic;

namespace SynthesisAPI.EventBus
{
    public static class EventBus
    {
        public delegate void EventCallback(IEvent eventInfo);

        /// <summary>
        /// Activates all callback functions that are listening to a particular
        /// IEvent type and passes event information to those functions 
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="eventInfo">Event to be passed to callback functions</param>
        /// <returns>True if message was pushed to subscribers, false if no subscribers were found</returns>
        public static bool Push<TEvent>(TEvent eventInfo) where TEvent : IEvent
        {
            string type = eventInfo.GetType().FullName;
            if (Instance.TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
            {
                Instance.TypeSubscribers[type](eventInfo);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Activates all callback functions that are listening to a particular
        /// tag or to the IEvent type of eventInfo and passes event information to 
        /// those listener functions 
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="eventInfo">Event to be passed to callback functions</param>
        /// <param name="tag">Tag corresponding to the group of listener functions to be activated</param>
        /// <returns>True if message was pushed to subscribers, false if no subscribers were found</returns>

        public static bool Push<TEvent>(string tag, TEvent eventInfo) where TEvent : IEvent
        {
            string type = eventInfo.GetType().FullName;
            if (Instance.TagSubscribers.ContainsKey(tag) && Instance.TagSubscribers[tag] != null)
            {
                Instance.TagSubscribers[tag](eventInfo);
                if (Instance.TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
                {
                    Instance.TypeSubscribers[type](eventInfo);
                }
                return true;
            }
            else if (Instance.TypeSubscribers.ContainsKey(eventInfo.EventType) && Instance.TypeSubscribers[type] != null)
            {
                Instance.TypeSubscribers[eventInfo.EventType](eventInfo);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Adds a callback function that is activated by an event of
        /// the specified type
        /// </summary>
        /// <typeparam name="TEvent">Type of event to listen for</typeparam>
        /// <param name="callback">The callback function to be activated</param>
        public static void NewTypeListener<TEvent>(EventCallback callback) where TEvent : IEvent
        {
            string type = typeof(TEvent).FullName;
            if (Instance.TypeSubscribers.ContainsKey(type))
                Instance.TypeSubscribers[type] += callback;
            else
                Instance.TypeSubscribers.Add(type, callback);
        }

        /// <summary>
        /// Adds a callback function that is activated by an event 
        /// being pushed to the specified tag
        /// </summary>
        /// <param name="tag">The tag to listen for</param>
        /// <param name="callback">The callback function to be activated</param>
        public static void NewTagListener(string tag, EventCallback callback)
        {
            if (Instance.TagSubscribers.ContainsKey(tag))
                Instance.TagSubscribers[tag] += callback;
            else
                Instance.TagSubscribers.Add(tag, callback);
        }

        /// <summary>
        /// Unsubscribes listener from receiving further events of specified type
        /// </summary>
        /// <typeparam name="TEvent">Type of event to stop listening for</typeparam>
        /// <param name="callback">The callback function to be removed</param>
        /// <returns>True if listener was successfully removed and false if type was not found</returns>
        public static bool RemoveTypeListener<TEvent>(EventCallback callback) where TEvent : IEvent
        {
            string type = typeof(TEvent).FullName;
            if (Instance.TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
            {
                Instance.TypeSubscribers[type] -= callback;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Unsubscribes listener from receiving further events of specified tag
        /// </summary>
        /// <param name="tag">The tag to stop listening for</param>
        /// <param name="callback">The callback function to be removed</param>
        ///  <returns>True if listener was successfully removed and false if tag was not found</returns>
        public static bool RemoveTagListener(string tag, EventCallback callback)
        {
            if (Instance.TagSubscribers.ContainsKey(tag) && Instance.TagSubscribers[tag] != null)
            {
                Instance.TagSubscribers[tag] -= callback;
                return true;
            }
            else
                return false;
        }

        public static void ResetAllListeners()
        {
            Instance.TypeSubscribers = new Dictionary<string, EventCallback>();
            Instance.TagSubscribers = new Dictionary<string, EventCallback>();
        }
        private class Inner
        {
            public Dictionary<string, EventCallback> TypeSubscribers;
            public Dictionary<string, EventCallback> TagSubscribers;

            private Inner()
            {
                TypeSubscribers = new Dictionary<string, EventCallback>();
                TagSubscribers = new Dictionary<string, EventCallback>();
            }

            private static Inner? _inst;
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

}