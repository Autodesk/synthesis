using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private Stack<SimState> activeStates;
        private Dictionary<Type, List<Behaviour>> stateBehaviours;
        private Dictionary<Type, List<GameObject>> stateGameObjects;

        /// <summary>
        /// Used to enable and disable objects associated with the current state.
        /// </summary>
        private bool CurrentObjectsEnabled
        {
            set
            {
                if (CurrentState == null)
                    return;

                Type currentType = CurrentState.GetType();

                if (stateBehaviours.ContainsKey(currentType))
                {
                    List<Behaviour> currentBehaviours = stateBehaviours[CurrentState.GetType()];

                    if (currentBehaviours != null)
                        foreach (Behaviour behaviour in currentBehaviours)
                            if (behaviour != null)
                                behaviour.enabled = value;
                }

                if (stateGameObjects.ContainsKey(currentType))
                {
                    List<GameObject> currentGameObjects = stateGameObjects[CurrentState.GetType()];

                    if (currentGameObjects != null)
                        foreach (GameObject gameObect in currentGameObjects)
                            if (gameObect != null)
                                gameObect.SetActive(value);
                }
            }
        }

        /// <summary>
        /// The current state in the StateMachine.
        /// </summary>
        public SimState CurrentState { get; private set; }

        /// <summary>
        /// The global StateMachine instance.
        /// </summary>
        public static StateMachine Instance
        {
            get
            {
                return (StateMachine)FindObjectOfType(typeof(StateMachine));
            }
        }

        /// <summary>
        /// Initializes the StateMachine.
        /// </summary>
        private StateMachine()
        {
            activeStates = new Stack<SimState>();
            stateBehaviours = new Dictionary<Type, List<Behaviour>>();
            stateGameObjects = new Dictionary<Type, List<GameObject>>();
        }

        /// <summary>
        /// Finds the states of the given type in the stack of active states.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindState<T>() where T : SimState
        {
            foreach (SimState state in activeStates)
                if (state is T)
                    return state as T;
            
            return null;
        }

        /// <summary>
        /// Links the given MonoBehaviour script to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the MonoBehaviour</typeparam>
        /// <param name="behaviour">The MonoBehaviour to link</param>
        public void Link<T>(MonoBehaviour behaviour) where T : SimState
        {
            if (!stateBehaviours.ContainsKey(typeof(T)))
                stateBehaviours[typeof(T)] = new List<Behaviour>();
            else if (stateBehaviours[typeof(T)].Contains(behaviour))
                return;

            behaviour.enabled = CurrentState is T;
            stateBehaviours[typeof(T)].Add(behaviour);
        }

        /// <summary>
        /// Links the given GameObject to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the GameObject</typeparam>
        /// <param name="gameObject">The GameObject to link</param>
        public void Link<T>(GameObject gameObject) where T : SimState
        {
            if (!stateGameObjects.ContainsKey(typeof(T)))
                stateGameObjects[typeof(T)] = new List<GameObject>();
            else if (stateGameObjects[typeof(T)].Contains(gameObject))
                return;

            gameObject.SetActive(CurrentState is T);
            stateGameObjects[typeof(T)].Add(gameObject);
        }

        /// <summary>
        /// Adds a new state to the StateMachine and pauses the current one if it exists.
        /// </summary>
        /// <param name="state"></param>
        public void PushState(SimState state)
        {
            if (CurrentState != null)
                CurrentState.Pause();

            CurrentObjectsEnabled = false;

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            CurrentState = state;

            CurrentObjectsEnabled = true;

            CurrentState.Start();
        }

        /// <summary>
        /// Removes the current state from the StateMachine.
        /// </summary>
        public void PopState()
        {
            if (CurrentState == null)
                return;

            CurrentState.Pause();
            CurrentState.End();

            CurrentObjectsEnabled = false;

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                CurrentState = activeStates.First();

                CurrentObjectsEnabled = true;

                CurrentState.Resume();
            }
            else
            {
                CurrentState = null;
            }
        }

        /// <summary>
        /// Creates the default state when the StateMachine is initialized.
        /// </summary>
        void Start()
        {
            if (CurrentState != null)
                return;

            PushState(new MainState());
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        void Update()
        {
            if (CurrentState != null)
                CurrentState.Update();
        }

        /// <summary>
        /// LateUpdates the current state.
        /// </summary>
        void LateUpdate()
        {
            if (CurrentState != null)
                CurrentState.LateUpdate();
        }

        /// <summary>
        /// FixedUpdates the current state.
        /// </summary>
        void FixedUpdate()
        {
            if (CurrentState != null)
                CurrentState.FixedUpdate();
        }

        /// <summary>
        /// Updates the GUI of the current state.
        /// </summary>
        [STAThread]
        void OnGUI()
        {
            if (CurrentState != null)
                CurrentState.OnGUI();
        }

        /// <summary>
        /// Calls the Awake method of the current state.
        /// </summary>
        void Awake()
        {
            if (CurrentState != null)
                CurrentState.Awake();
        }
    }
}