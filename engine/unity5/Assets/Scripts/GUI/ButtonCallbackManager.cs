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
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// This callback manager is used for registering Unity button and dropdown callbacks for the tab StateMachine.
    /// </summary>
    public static class ButtonCallbackManager
    {
        /// <summary>
        /// Finds each Button component in the main menu that doesn't already have a
        /// listener and registers it with a callback.
        /// </summary>
        public static void RegisterButtonCallbacks(StateMachine stateMachine, GameObject uiRoot)
        {
            foreach (Button b in uiRoot.GetComponentsInChildren<Button>(true))
                if (b.onClick.GetPersistentEventCount() == 0)
                    b.onClick.AddListener(() => InvokeCallback(stateMachine, "On" + b.name + "Pressed"));
        }

        /// <summary>
        /// Finds each Dropdown component in the main menu that doesn't already have a
        /// listener and registers it with a callback.
        /// </summary>
        public static void RegisterDropdownCallbacks(StateMachine stateMachine, GameObject dropdownRoot)
        {
            //Add listener for when the value of the Dropdown changes, to take action
            foreach (Dropdown d in dropdownRoot.GetComponentsInChildren<Dropdown>(true))
                if (d.onValueChanged.GetPersistentEventCount() == 0)
                    d.onValueChanged.AddListener((index) => InvokeCallback(stateMachine, "On" + d.name + "Clicked", index));
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