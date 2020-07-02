using System;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Axis;
using SynthesisAPI.InputManager.Events;
using SynthesisAPI.EventBus;
using PM = SynthesisAPI.PreferenceManager.PreferenceManager;
using UnityInput = UnityEngine.Input;
using SynthesisAPI.PreferenceManager;

namespace SynthesisAPI.InputManager
{
    /**
     * Still very much a work in progress
     */
    public static class InputManager
    {
        private static readonly Guid MyGuid = Guid.NewGuid();

        // Map for binding DigitalInput to EventHandlers
        private static BiDictionary<IDigitalInput, string> _mappedDigital = new BiDictionary<IDigitalInput, string>();
        // Used for giving custom names to axes
        private static Dictionary<string, IAxisInput> _mappedAxes = new Dictionary<string, IAxisInput>();

        // Used to identify controller type because ps4 be wack
        public static Dictionary<int, ControllerType> ControllerRegistry = new Dictionary<int, ControllerType>();
        public static string[] LastControllerNames = new string[1];

        static InputManager()
        {
            // TODO: Find new way of attaching to update loop
            // _ = InputGlobalBehavior.Instance;

            for (int i = 1; i <= 11; i++)
            {
                ControllerRegistry.Add(i, ControllerType.Other); // Default all the controllers to other
            }

            // So we can start detecting changes in controllers;
            LastControllerNames = UnityInput.GetJoystickNames();
            EvaluateControllerTypes();

            // _mappedDigital[(KeyDigital)new [] { "A" }] = "test_action";

            // Assign general axes. (Do I need this???)
            _mappedAxes["MouseX"] = (DualAxis)"Mouse X";
            _mappedAxes["MouseY"] = (DualAxis)"Mouse Y";

            // TODO: Subscribe PostLoad and PreSave to corresponding events
            
            EventBus.EventBus.NewTagListener("prefs/io", e =>
            {
                if ((PreferencesIOEvent.Status)e.GetArguments()[0] == PreferencesIOEvent.Status.PreSave)
                    PreSave();
                else
                    PostLoad();
            });
        }

        public static void AssignAxis(string name, IAxisInput input) => _mappedAxes[name] = input;

        public static float GetAxisValue(string axisName, bool positiveOnly = false) =>
            _mappedAxes.ContainsKey(axisName)? _mappedAxes[axisName].GetValue(positiveOnly) : 0.0f;

        public static void AssignDigital(string name, IDigitalInput input, EventBus.EventBus.EventCallback? callback = null)
        {
            _mappedDigital[name] = input;
            if (callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void AssignCallback(string name, EventBus.EventBus.EventCallback callback) =>
            EventBus.EventBus.NewTagListener($"input/{name}", callback);

        private static void PostLoad()
        {
            _mappedDigital = PM.GetPreference<BiDictionary<IDigitalInput, string>>(MyGuid, "all_bindings", useJsonDeserialization: true);
        }

        private static void PreSave()
        {
            PM.SetPreference(MyGuid, "all_bindings", _mappedDigital);
        }

        #region Getting active inputs

        public static IDigitalInput GetCurrentlyActiveDigitalInput((int joystick, int button)[] buttonsToIgnore = null, KeyCode[] keysToIgnore = null)
        {
            ButtonDigital activeButtonDigital;
            KeyDigital activeKeyDigital;

            if (buttonsToIgnore == null) activeButtonDigital = ButtonDigital.GetCurrentlyActiveButtonDigital();
            else activeButtonDigital = ButtonDigital.GetCurrentlyActiveButtonDigital(buttonsToIgnore);
            if (keysToIgnore == null) activeKeyDigital = KeyDigital.GetCurrentlyActiveKeyDigital();
            else activeKeyDigital = KeyDigital.GetCurrentlyActiveKeyDigital(keysToIgnore);

            if (activeKeyDigital.Length > 0) return activeKeyDigital;
            else if (activeButtonDigital.Length > 0) return activeButtonDigital;
            else return null;
        }

        public static IAxisInput GetCurrentlyActiveAxisInput((int joystick, int button)[] buttonsToIgnore = null, KeyCode[] keysToIgnore = null, string[] axesToIgnore = null)
        {
            ButtonDigital activeButtonDigital;
            KeyDigital activeKeyDigital;
            UnityAxis activeUnityAxis;
            JoystickAxis activeJoystickAxis;

            if (buttonsToIgnore == null)
            {
                activeButtonDigital = ButtonDigital.GetCurrentlyActiveButtonDigital();
            }
            else
            {
                activeButtonDigital = ButtonDigital.GetCurrentlyActiveButtonDigital(buttonsToIgnore);
            }
            if (keysToIgnore == null)
            {
                activeKeyDigital = KeyDigital.GetCurrentlyActiveKeyDigital();
            }
            else
            {
                activeKeyDigital = KeyDigital.GetCurrentlyActiveKeyDigital(keysToIgnore);
            }
            if (axesToIgnore == null)
            {
                activeUnityAxis = UnityAxis.GetCurrentlyActiveUnityAxis();
                activeJoystickAxis = JoystickAxis.GetCurrentlyActiveJoystickAxis();
            }
            else
            {
                activeUnityAxis = UnityAxis.GetCurrentlyActiveUnityAxis(axesToIgnore);
                activeJoystickAxis = JoystickAxis.GetCurrentlyActiveJoystickAxis(axesToIgnore);
            }

            if (activeUnityAxis != null) { return activeUnityAxis; }
            else if (activeJoystickAxis != null) { return activeJoystickAxis; }
            else if (activeKeyDigital.Length > 0) { return activeKeyDigital; }
            else if (activeButtonDigital.Length > 0) { return activeButtonDigital; }
            return null;
        }

        #endregion

        #region Conversions

        private static readonly List<KeyCode> allKeys = new List<KeyCode>((KeyCode[])Enum.GetValues(typeof(KeyCode)));
        public static int ToInt(this KeyCode k)
        {
            return allKeys.IndexOf(k);
        }
        public static int[] ToIntArray(this KeyCode[] ks)
        {
            int[] result = new int[ks.Length];
            for (int i = 0; i < ks.Length; i++)
            {
                result[i] = allKeys.IndexOf(ks[i]);
            }
            return result;
        }

        public static string ToButtonName(this (int joystick, int button) b)
        {
            return "joystick " + b.joystick + " button " + b.button;
        }

        public static string[] ToButtonNameArray(this (int joystick, int button)[] buttons)
        {
            string[] names = new string[buttons.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = buttons[i].ToButtonName();
            }
            return names;
        }

        #endregion

        #region Controller configuration

        public enum ControllerType
        {
            Ps4, Other
        }

        /// <summary>
        /// Evaluates the type of all the controllers. There isn't an effective way
        /// of isolating this process to just newly connected/disconnected controllers
        /// so this will just be run when it we detect any change in controllers.
        /// TODO: Maybe publish an event for when controllers are re-evaluated.
        /// </summary>
        public static void EvaluateControllerTypes()
        {
            for (int i = 1; i <= 11; i++)
            {
                if (UnityInput.GetAxis("Joystick " + i + " Axis 4") < -0.9
                    && UnityInput.GetAxis("Joystick " + i + " Axis 5") < -0.9)
                {
                    ControllerRegistry[i] = ControllerType.Ps4;
                    Debug.Log(i + " is Ps4");
                } else
                {
                    ControllerRegistry[i] = ControllerType.Other;
                }
            }
        }

        #endregion
        
        public enum DigitalState
        {
            Up = 1, Down = 2, Held = 3, None = 0
        }
        
        #region Systems
        
        /// <summary>
        /// A behavior to actively update the InputManager
        /// TODO: Attach to a global object and keep this class to a single instance
        /// </summary>
        public class InputSystem : SystemBase
        {
            /// <summary>
            /// Updates all the key presses.
            /// TODO: Publish axes to event bus as well.
            /// </summary>
            public override void Update()
            {
                InputManager.DigitalState inputState;
                foreach (var kvp in InputManager._mappedDigital)
                {
                    inputState = kvp.Key.GetState();
                    if (inputState != InputManager.DigitalState.None)
                    {
                        // EventBus.Publish(new DigitalStateEvent(kvp.Value, inputState))
                        // TODO: Publish to event bus with value identifier and state
                        EventBus.EventBus.Push($"input/{kvp.Value}",
                            new DigitalStateEvent(kvp.Value, inputState));
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

        public class InputGlobalSystem : GlobalSystem<InputSystem>
        {
            public InputGlobalSystem() : base() { }
        }
        
        #endregion
    }
    
    
}