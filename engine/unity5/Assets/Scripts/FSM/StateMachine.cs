using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Synthesis.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private static StateMachine sceneGlobal;

        private Stack<State> activeStates;
        private readonly Dictionary<Type, Tuple<bool, HashSet<Behaviour>>> stateBehaviours;
        private readonly Dictionary<Type, Tuple<bool, HashSet<GameObject>>> stateGameObjects;

        /// <summary>
        /// (Defined from the editor)
        /// If true, this instance can be accessed globally from the <see cref="SceneGlobal"/> property.
        /// </summary>
        public bool sceneGlobalInstance;
        
        /// <summary>
        /// The global StateMachine instance.
        /// </summary>
        public static StateMachine SceneGlobal
        {
            get
            {
                if (sceneGlobal == null)
                    Debug.LogWarning("No global StateMachine instance has been defined!");

                return sceneGlobal;
            }
            private set
            {
                sceneGlobal = value;
            }
        }

        /// <summary>
        /// The current state in the StateMachine.
        /// </summary>
        public State CurrentState { get; private set; }

        /// <summary>
        /// Initializes the StateMachine.
        /// </summary>
        private StateMachine()
        {
            activeStates = new Stack<State>();
            stateBehaviours = new Dictionary<Type, Tuple<bool, HashSet<Behaviour>>>();
            stateGameObjects = new Dictionary<Type, Tuple<bool, HashSet<GameObject>>>();
        }

        /// <summary>
        /// Finds the states of the given type in the stack of active states.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindState<T>() where T : State
        {
            foreach (State state in activeStates)
                if (state is T)
                    return state as T;
            
            return null;
        }

        /// <summary>
        /// Links the given MonoBehaviour script to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the <see cref="Behaviour"/></typeparam>
        /// <param name="behaviour">The MonoBehaviour to link</param>
        public void Link<T>(Behaviour behaviour, bool strict = true) where T : State
        {
            if (Link<Behaviour, T>(stateBehaviours, behaviour, strict))
                behaviour.enabled = CurrentState is T;
        }

        /// <summary>
        /// Links the given GameObject to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the GameObject</typeparam>
        /// <param name="gameObject">The GameObject to link</param>
        /// <param name="strict">When true, this GameObject should be disabled when a <see cref="State"/>
        /// is pushed before the current one is popped.</param>
        public void Link<T>(GameObject gameObject, bool strict = true) where T : State
        {
            if (Link<GameObject, T>(stateGameObjects, gameObject, strict))
                gameObject.SetActive(CurrentState is T);
        }

        /// <summary>
        /// Adds a new state to the StateMachine and pauses the current one if it exists.
        /// </summary>
        /// <param name="state"></param>
        public void PushState(State state)
        {
            if (CurrentState != null)
                CurrentState.Pause();

            SetObjectsEnabled(false, false);

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            CurrentState = state;
            CurrentState.StateMachine = this;
            CurrentState.Awake();

            SetObjectsEnabled(true);

            CurrentState.Start();
            CurrentState.Resume();
        }

        /// <summary>
        /// Removes the current state from the StateMachine.
        /// </summary>
        public bool PopState()
        {
            if (CurrentState == null)
                return false;

            CurrentState.Pause();
            CurrentState.End();
            CurrentState.StateMachine = null;

            SetObjectsEnabled(false);

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                CurrentState = activeStates.First();

                SetObjectsEnabled(true);

                CurrentState.Resume();
            }
            else
            {
                CurrentState = null;
            }

            return true;
        }

        /// <summary>
        /// Pops all open States and pushes the given State.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(State state)
        {
            while (PopState()) ;
            PushState(state);
        }

        /// <summary>
        /// Links the given item to the type of <see cref="State"/> specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="items"></param>
        /// <param name="item"></param>
        /// <param name="strict"></param>
        /// <returns></returns>
        private bool Link<T, S>(Dictionary<Type, Tuple<bool, HashSet<T>>> items, T item, bool strict)
        {
            if (!items.ContainsKey(typeof(S)))
                items[typeof(S)] = Tuple.Create(strict, new HashSet<T>());
            else if (items[typeof(S)].Item2.Contains(item))
                return false;

            items[typeof(S)].Item2.Add(item);

            return true;
        }

        /// <summary>
        /// Used to enable and disable objects associated with the current state.
        /// </summary>
        /// <param name="objectsEnabled">true if the object should be enabled, otherwise false</param>
        /// <param name="force">If true, then objects should always be disabled when
        /// enabled = false, even if they have non-strict linking</param>
        private bool SetObjectsEnabled(bool objectsEnabled, bool force = true)
        {
            if (CurrentState == null)
                return false;

            SetEnabled(stateBehaviours, force, (behaviour) => behaviour.enabled = objectsEnabled);
            SetEnabled(stateGameObjects, force, (gameObject) => gameObject.SetActive(objectsEnabled));

            return true;
        }

        /// <summary>
        /// Enables or disables the given items via the callback method provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="force"></param>
        /// <param name="setItemEnabled"></param>
        private void SetEnabled<T>(Dictionary<Type, Tuple<bool, HashSet<T>>> items, bool force, Action<T> setItemEnabled)
        {
            Type currentType = CurrentState.GetType();
            Tuple<bool, HashSet<T>> currentItems;

            if (!items.ContainsKey(currentType) || (currentItems = items[currentType]) == null)
                return;

            currentItems.Item2.RemoveWhere(o => o.Equals(null));

            if (!force && !currentItems.Item1)
                return;

            foreach (T value in currentItems.Item2)
                setItemEnabled(value);
        }

        /// <summary>
        /// Sets the Instance property to this active instance.
        /// </summary>
        private void Awake()
        {
            if (sceneGlobalInstance)
                SceneGlobal = this;
        }

        /// <summary>
        /// Resets the scene global instance when the <see cref="StateMachine"/> is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            SceneGlobal = null;
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        private void Update()
        {
            CurrentState?.Update();
        }

        /// <summary>
        /// LateUpdates the current state.
        /// </summary>
        private void LateUpdate()
        {
            CurrentState?.LateUpdate();
        }

        /// <summary>
        /// FixedUpdates the current state.
        /// </summary>
        private void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        /// <summary>
        /// Updates the GUI of the current state.
        /// </summary>
        private void OnGUI()
        {
            CurrentState?.OnGUI();
        }
    }
}