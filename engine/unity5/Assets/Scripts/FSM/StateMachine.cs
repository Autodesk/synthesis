﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneType = UnityEngine.SceneManagement.Scene;

namespace Assets.Scripts.FSM
{
    public class StateMachine : MonoBehaviour
    {
        private Stack<SimState> activeStates;
        private Dictionary<Type, List<MonoBehaviour>> stateBehaviours;

        /// <summary>
        /// Used to enable and disable behaviours associated with the current state.
        /// </summary>
        private bool CurrentBehavioursEnabled
        {
            set
            {
                if (CurrentState == null)
                    return;

                Type currentType = CurrentState.GetType();

                if (!stateBehaviours.ContainsKey(currentType))
                    return;

                List<MonoBehaviour> currentBehaviours = stateBehaviours[CurrentState.GetType()];

                if (currentBehaviours != null)
                    foreach (MonoBehaviour behaviour in currentBehaviours)
                        behaviour.enabled = value;
            }
        }

        /// <summary>
        /// The current state in the StateMachine.
        /// </summary>
        public SimState CurrentState { get; private set; }

        public Dictionary<string, SimState> DefaultSceneState = new Dictionary<string, SimState>()
        {
            {"Scene", new MainState()},
            {"EditScoreZones", new EditScoreZoneState()}
        };

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
            stateBehaviours = new Dictionary<Type, List<MonoBehaviour>>();
        }

        /// <summary>
        /// Links the given MonoBehaviour script to the provided state type.
        /// </summary>
        /// <typeparam name="T">The type of state with which to link the MonoBehaviour</typeparam>
        /// <param name="behaviour">The MonoBehaviour to link</param>
        public void LinkBehaviour<T>(MonoBehaviour behaviour) where T : SimState
        {
            if (!stateBehaviours.ContainsKey(typeof(T)))
                stateBehaviours[typeof(T)] = new List<MonoBehaviour>();
            else if (stateBehaviours[typeof(T)].Contains(behaviour))
                return;

            stateBehaviours[typeof(T)].Add(behaviour);
        }

        /// <summary>
        /// Adds a new state to the StateMachine and pauses the current one if it exists.
        /// </summary>
        /// <param name="state"></param>
        public void PushState(SimState state)
        {
            if (CurrentState != null)
                CurrentState.Pause();

            CurrentBehavioursEnabled = false;

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            CurrentState = state;

            CurrentBehavioursEnabled = true;

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

            CurrentBehavioursEnabled = false;

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                CurrentState = activeStates.First();

                CurrentBehavioursEnabled = true;

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

            // PushState(new MainState());
            
            foreach (string key in DefaultSceneState.Keys)
            {
                Debug.Log("Checking " + key + " and " + SceneManager.GetActiveScene().name);
                if (key == SceneManager.GetActiveScene().name)
                {
                    Debug.Log("Match");
                    PushState(DefaultSceneState[key]);
                    break;
                }
            }
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
