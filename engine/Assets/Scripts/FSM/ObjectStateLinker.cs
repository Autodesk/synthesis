using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.FSM
{
    public class ObjectStateLinker<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Describes settings for the link between an object and a State.
        /// </summary>
        private class LinkDescriptor
        {
            /// <summary>
            /// If true, the associated object will be enabled when its state becomes active.
            /// </summary>
            public bool EnableWithState { get; set; }

            /// <summary>
            /// If true, this object will be disabled after a new state is pushed on top of its
            /// parent state.
            /// </summary>
            public bool UseStrictLinking { get; set; }

            /// <summary>
            /// Initializes a new <see cref="LinkDescriptor"/> instance.
            /// </summary>
            /// <param name="enableWithState"></param>
            /// <param name="useStrictLinking"></param>
            public LinkDescriptor(bool enableWithState, bool useStrictLinking)
            {
                EnableWithState = enableWithState;
                UseStrictLinking = useStrictLinking;
            }
        }

        private readonly Action<T, bool> setEnabled;
        private readonly Func<T, bool> getEnabled;
        private readonly Dictionary<Type, Dictionary<T, LinkDescriptor>> stateObjects;

        /// <summary>
        /// Initializes a new <see cref="ObjectStateLinker{T}"/> instance.
        /// </summary>
        /// <param name="setEnabled"></param>
        /// <param name="getEnabled"></param>
        public ObjectStateLinker(Action<T, bool> setEnabled, Func<T, bool> getEnabled)
        {
            this.setEnabled = setEnabled;
            this.getEnabled = getEnabled;

            stateObjects = new Dictionary<Type, Dictionary<T, LinkDescriptor>>();
        }

        /// <summary>
        /// Links the object provided to the specified state type.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="obj"></param>
        /// <param name="inActiveState"></param>
        /// <param name="enableWithState"></param>
        /// <param name="useStrictLinking"></param>
        public void Link<S>(T obj, bool inActiveState, bool enableWithState, bool useStrictLinking)
        {
            if (!stateObjects.ContainsKey(typeof(S)))
                stateObjects[typeof(S)] = new Dictionary<T, LinkDescriptor>();
            else if (stateObjects[typeof(S)].ContainsKey(obj))
                return;

            stateObjects[typeof(S)].Add(obj, new LinkDescriptor(enableWithState, useStrictLinking));

            setEnabled(obj, inActiveState && enableWithState);
        }

        /// <summary>
        /// Enables all objects associated with the given type of state.
        /// </summary>
        /// <param name="stateType"></param>
        public void EnableObjects(Type stateType)
        {
            Dictionary<T, LinkDescriptor> currentObjects = GetObjectsFromStateType(stateType);

            if (currentObjects == null)
                return;

            foreach (T key in currentObjects.Where(o => o.Value.EnableWithState)
                .Select(o => o.Key))
                setEnabled(key, true);
        }

        /// <summary>
        /// Disables all objects associated with the given type of state.
        /// </summary>
        /// <param name="stateType"></param>
        /// <param name="force"></param>
        public void DisableObjects(Type stateType, bool force)
        {
            Dictionary<T, LinkDescriptor> currentObjects = GetObjectsFromStateType(stateType);

            if (currentObjects == null)
                return;

            foreach (T key in (force ? currentObjects.Keys :
                currentObjects.Where(o => o.Value.UseStrictLinking).Select(o => o.Key)))
            {
                currentObjects[key].EnableWithState = getEnabled(key);
                setEnabled(key, false);
            }
        }

        /// <summary>
        /// Finds all objects associated with the state type provided, cleans out any
        /// lost references, and returns the updated dictionary.
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        private Dictionary<T, LinkDescriptor> GetObjectsFromStateType(Type stateType)
        {
            Dictionary<T, LinkDescriptor> currentObjects;

            if (!stateObjects.ContainsKey(stateType) || (currentObjects = stateObjects[stateType]) == null)
                return null;

            foreach (T key in currentObjects.Keys.Where(o => o.Equals(null)).ToList())
                currentObjects.Remove(key);

            return currentObjects;
        }
    }
}
