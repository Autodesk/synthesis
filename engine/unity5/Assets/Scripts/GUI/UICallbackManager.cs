using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    public static class UICallbackManager
    {
        /// <summary>
        /// Finds each Button component in that doesn't already have a
        /// listener and registers it with a callback.
        /// </summary>
        public static void RegisterButtonCallbacks(StateMachine stateMachine, GameObject uiRoot)
        {
            RegisterCallbacks<Button>(stateMachine, uiRoot, b => b.onClick);
        }

        /// <summary>
        /// Finds each Dropdown component that doesn't already have a listener and registers it with
        /// a callback.
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <param name="uiRoot"></param>
        public static void RegisterDropdownCallbacks(StateMachine stateMachine, GameObject uiRoot)
        {
            RegisterCallbacks<Dropdown, int>(stateMachine, uiRoot, d => d.onValueChanged);
        }

        /// <summary>
        /// Finds each Toggle component that doesn't already have a listener and registers it with
        /// a callback.
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <param name="uiRoot"></param>
        public static void RegisterToggleCallbacks(StateMachine stateMachine, GameObject uiRoot)
        {
            RegisterCallbacks<Toggle, bool>(stateMachine, uiRoot, t => t.onValueChanged);
        }

        /// <summary>
        /// Registers the given button with a callback for the given <see cref="StateMachine"/>.
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <param name="button"></param>
        private static void RegisterClickCallback(StateMachine stateMachine, UnityEvent unityEvent, string selectableName)
        {
            unityEvent.AddListener(() => InvokeCallback(stateMachine,
                "On" + selectableName + "Clicked"));
        }

        /// <summary>
        /// Registers the given dropdown with a callback for the given <see cref="StateMachine"/>.s
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <param name="dropdown"></param>
        private static void RegisterValueChangedCallback<T>(StateMachine stateMachine, UnityEvent<T> unityEvent, string selectableName)
        {
            unityEvent.AddListener(arg => InvokeCallback(stateMachine,
                "On" + selectableName + "ValueChanged", arg));
        }
        
        /// <summary>
        /// Registers callbacks for the given type of <see cref="Selectable"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stateMachine"></param>
        /// <param name="uiRoot"></param>
        /// <param name="getEvent"></param>
        /// <param name="registerCallback"></param>
        private static void RegisterCallbacks<T, U>(StateMachine stateMachine, GameObject uiRoot,
            Func<T, U> getEvent, Action<StateMachine, U, string> registerCallback) where T : Selectable where U : UnityEventBase
        {
            foreach (T selectable in uiRoot.GetComponentsInChildren<T>(true))
            {
                U unityEvent = getEvent(selectable);

                if (unityEvent.GetPersistentEventCount() == 0)
                    registerCallback(stateMachine, unityEvent, selectable.name);
            }
        }

        /// <summary>
        /// Registers callbacks for the given type of <see cref="Selectable"/>.
        /// </summary>
        /// <typeparam name="T">The type of selectable to register callbacks for.</typeparam>
        /// <param name="stateMachine"></param>
        /// <param name="uiRoot"></param>
        /// <param name="getEvent"></param>
        private static void RegisterCallbacks<T>(StateMachine stateMachine, GameObject uiRoot,
            Func<T, UnityEvent> getEvent) where T : Selectable
        {
            RegisterCallbacks(stateMachine, uiRoot, getEvent, RegisterClickCallback);
        }

        /// <summary>
        /// Registers callbacks for the given type of <see cref="Selectable"/>.
        /// </summary>
        /// <typeparam name="T">The type of selectable to register callbacks for.</typeparam>
        /// <typeparam name="U">The type of parameter passed to the selectable's value changed callback.</typeparam>
        /// <param name="stateMachine"></param>
        /// <param name="uiRoot"></param>
        /// <param name="getEvent"></param>
        private static void RegisterCallbacks<T, U>(StateMachine stateMachine, GameObject uiRoot,
            Func<T, UnityEvent<U>> getEvent) where T : Selectable
        {
            RegisterCallbacks(stateMachine, uiRoot, getEvent, RegisterValueChangedCallback);
        }

        /// <summary>
        /// Invokes a method in the active <see cref="State"/> by the given method name.
        /// </summary>
        /// <param name="methodName"></param>
        private static void InvokeCallback(StateMachine stateMachine, string methodName, params object[] args)
        {
            State currentState = stateMachine.CurrentState;
            MethodInfo info = currentState.GetType().GetMethod(methodName);

            if (info == null)
                Debug.LogWarning("Method " + methodName + " does not have a listener in " + currentState.GetType().ToString());
            else
                info.Invoke(currentState, args);
        }
    }
}