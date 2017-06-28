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
        private SimState currentState;
        
        public static StateMachine Instance
        {
            get
            {
                return (StateMachine)FindObjectOfType(typeof(StateMachine));
            }
        }

        public StateMachine()
        {
            activeStates = new Stack<SimState>();
        }

        public void PushState(SimState state)
        {
            if (currentState != null)
                currentState.Pause();

            if (!activeStates.Contains(state))
                activeStates.Push(state);

            currentState = state;

            currentState.Start();
            currentState.Resume();
        }

        public void PopState()
        {
            if (currentState == null)
                return;

            currentState.Pause();
            currentState.End();

            activeStates.Pop();

            if (activeStates.Count > 0)
            {
                currentState = activeStates.Last();

                currentState.Resume();
            }
            else
            {
                currentState = null;
            }
        }

        void Start()
        {
            if (currentState != null)
                return;

            string defaultStateName = EditorPrefs.GetString(StateMachineEditor.DefaultStateNameKey);
            Type defaultStateType = Type.GetType(defaultStateName);

            if (defaultStateType == null)
            {
                Debug.LogError("\"" + defaultStateName + "\" is not a valid type!");
                return;
            }

            SimState defaultState = Activator.CreateInstance(defaultStateType) as SimState;

            if (defaultState == null)
                Debug.LogError("\"" + defaultStateName + "\" does not extend SimState!");
            else
                PushState(defaultState);
            
        }

        void Update()
        {
            if (currentState != null)
                currentState.Update();
        }

        void FixedUpdate()
        {
            if (currentState != null)
                currentState.FixedUpdate();
        }

        [STAThread]
        void OnGUI()
        {
            if (currentState != null)
                currentState.OnGUI();
        }

        void Awake()
        {
            if (currentState != null)
                currentState.Awake();
        }
    }
}
