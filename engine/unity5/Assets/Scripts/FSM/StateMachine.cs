using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private Stack<SimState> activeStates;
<<<<<<< HEAD
        private SimState currentState;

        private MainState mainState;
        
=======

        /// <summary>
        /// The current state in the StateMachine.
        /// </summary>
        public SimState CurrentState { get; private set; }

        public MainState MainState { get; private set; }

        /// <summary>
        /// The global StateMachine instance.
        /// </summary>
>>>>>>> master
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
        }

        /// <summary>
        /// Adds a new state to the StateMachine and pauses the current one if it exists.
        /// </summary>
        /// <param name="state"></param>
        public void PushState(SimState state)
        {
            if (CurrentState != null)
                CurrentState.Pause();

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            CurrentState = state;

            CurrentState.Start();
            CurrentState.Resume();
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

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                CurrentState = activeStates.Last();

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

            string defaultStateName = EditorPrefs.GetString(StateMachineEditor.DefaultStateNameKey);
            Type defaultStateType = Type.GetType(defaultStateName);

            if (defaultStateType == null)
            {
                Debug.LogError("\"" + defaultStateName + "\" is not a valid type!");
                return;
            }

            SimState defaultState = Activator.CreateInstance(defaultStateType) as SimState;
<<<<<<< HEAD
            mainState = (MainState)defaultState;
=======
            MainState = (MainState)defaultState;
>>>>>>> master

            if (defaultState == null)
                Debug.LogError("\"" + defaultStateName + "\" does not extend SimState!");
            else
                PushState(defaultState);

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

        public MainState GetMainState()
        {
            return mainState;
        }
    }
}