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
            string type = eventInfo.GetType().ToString();
            if (Instance.typeSubscribers.ContainsKey(type) && Instance.typeSubscribers[type] != null)
            {
                Instance.typeSubscribers[type](eventInfo);
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
            string type = eventInfo.GetType().ToString();
            if (Instance.tagSubscribers.ContainsKey(tag) && Instance.tagSubscribers[tag] != null)
            {
                Instance.tagSubscribers[tag](eventInfo);
                if (Instance.typeSubscribers.ContainsKey(type) && Instance.typeSubscribers[type] != null)
                {
                    Instance.typeSubscribers[type](eventInfo);
                }
                return true;
            }
            else if (Instance.typeSubscribers.ContainsKey(eventInfo.EventType) && Instance.typeSubscribers[type] != null)
            {
                Instance.typeSubscribers[eventInfo.EventType](eventInfo);
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
            string type = typeof(TEvent).ToString();
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

        //Removal of listeners 

        /// <summary>
        /// Unsubscribes listener from recieving further events of specified type
        /// </summary>
        /// <typeparam name="TEvent">Type of event to stop listening for</typeparam>
        /// <param name="callback">The callback function to be removed</param>
        /// <returns>True if listener was successfully removed and false if type was not found</returns>
        public static bool RemoveTypeListener<TEvent>(EventCallback callback) where TEvent : IEvent
        {
            string type = typeof(TEvent).ToString();
            if (Instance.typeSubscribers.ContainsKey(type) && Instance.typeSubscribers[type] != null)
            {
                Instance.typeSubscribers[type] -= callback;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Adds a callback function that is activated by an event 
        /// being pushed to the specified tag
        /// </summary>
        /// <param name="tag">The tag to listen for</param>
        /// <param name="callback">The callback function to be activated</param>
        public static bool RemoveTagListener(string tag, EventCallback callback)
        {
            if (Instance.tagSubscribers.ContainsKey(tag) && Instance.tagSubscribers[tag] != null)
            {
                Instance.tagSubscribers[tag] -= callback;
                return true;
            }
            else
                return false;
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

}