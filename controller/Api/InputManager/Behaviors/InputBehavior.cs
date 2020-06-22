using System;
using SynthesisAPI.Modules;
using SynthesisAPI.InputManager.Digital;
using UnityInput = UnityEngine.Input;

namespace SynthesisAPI.InputManager.Behaviors
{
    /// <summary>
    /// A behavior to actively update the InputManager
    /// TODO: Attach to a global object and keep this class to a single instance
    /// </summary>
    public class InputBehavior : Behavior
    {
        /// <summary>
        /// Updates all the key presses.
        /// TODO: Publish axes to event bus as well.
        /// </summary>
        public override void Update()
        {
            IDigitalInput.DigitalState inputState;
            foreach (var kvp in InputManager.MappedDigital)
            {
                inputState = kvp.Key.GetState();
                if (inputState != IDigitalInput.DigitalState.None)
                {
                    // EventBus.Publish(new DigitalStateEvent(kvp.Value, inputState))
                    // TODO: Publish to event bus with value identifier and state
                }
            }
        }

        /// <summary>
        /// Checks for controller change, and if so, re-evaluates which controllers
        /// are ps4 and which aren't.
        /// </summary>
        public override void FixedUpdate()
        {
            int res = InputManager.LastControllerNames.Length;
            string[] currentNames = UnityInput.GetJoystickNames();
            if (res == currentNames.Length)
            {
                for (int i = 0; i < currentNames.Length; i++)
                {
                    if (currentNames[i].Equals(InputManager.LastControllerNames[i])) res -= 1;
                }
            }
            if (res != 0)
            {
                InputManager.LastControllerNames = currentNames;
                InputManager.EvaluateControllerTypes();
            }
        }
    }


}
