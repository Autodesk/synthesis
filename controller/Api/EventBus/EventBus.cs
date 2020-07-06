using System;
using System.Collections.Generic;
using System.Linq;

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
            string type = eventInfo.Name();
            if (Instance.TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
            {
                Instance.TypeSubscribers[type](eventInfo);
                return true;
            }
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
            string type = eventInfo.Name();
            if (Instance.TagSubscribers.ContainsKey(tag) && Instance.TagSubscribers[tag] != null)
            {
                Instance.TagSubscribers[tag](eventInfo);
                Push<TEvent>(eventInfo);
                return true;
            }
            if (Instance.TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
            {
                Instance.TypeSubscribers[type](eventInfo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Activates all callback functions that are listening to a particular
        /// set of tags or to the IEvent type of eventInfo and passes event
        /// information to those listener functions 
        /// </summary>
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="eventInfo">Event to be passed to callback functions</param>
        /// <param name="tags">Tags corresponding to the group of listener functions to be activated</param>
        /// <returns>True if message was pushed to subscribers, false if no subscribers were found</returns>
        public static bool Push<TEvent>(IEnumerable<string> tags, TEvent eventInfo) where TEvent : IEvent
        {
            bool pushed = false;
            foreach (var tag in tags)
            {
                pushed |= Push<TEvent>(tag, eventInfo);
            }
            return pushed;
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
            if (TypeSubscribers.ContainsKey(type) && Instance.TypeSubscribers[type] != null)
            {
                if (Instance.TypeSubscribers[type].GetInvocationList().Contains(callback))
                {
                    // ReSharper disable once DelegateSubtraction
                    TypeSubscribers[type] -= callback;
                    return true;
                }
                return false;
            }
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
            if (TagSubscribers.ContainsKey(tag) && TagSubscribers[tag] != null)
            {
                if (TagSubscribers[tag].GetInvocationList().Contains(callback)) {
                    TagSubscribers[tag] -= callback;
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void ResetAllListeners()
        {
            Inner.ResetAllListeners();
        }

        private class Inner
        {
            public Dictionary<string, EventCallback> TypeSubscribers;
            public Dictionary<string, EventCallback> TagSubscribers;

            public static void ResetAllListeners()
            {
                if (AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.FullName.ToLowerInvariant().StartsWith("nunit.framework")))
                {
                    _inst!.TypeSubscribers = new Dictionary<string, EventCallback>();
                    _inst!.TagSubscribers = new Dictionary<string, EventCallback>();
                }
                else
                {
                    throw new Exception("Cannot reset all listeners outside of test suite");
                }
            }

            private Inner()
            {
                TypeSubscribers = new Dictionary<string, EventCallback>();
                TagSubscribers = new Dictionary<string, EventCallback>();
            }

            private static Inner? _inst;
            public static Inner InnerInstance => _inst ??= new Inner();
        }

        private static Dictionary<string, EventCallback> TypeSubscribers => Instance.TypeSubscribers;
        private static Dictionary<string, EventCallback> TagSubscribers => Instance.TagSubscribers;

        private static Inner Instance => Inner.InnerInstance;
    }

}