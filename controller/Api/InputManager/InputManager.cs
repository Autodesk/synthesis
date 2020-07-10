using System;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Utilities;
using SynthesisAPI.EventBus;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Axis;
using SynthesisAPI.InputManager.Events;
using PM = SynthesisAPI.PreferenceManager.PreferenceManager;
using UnityInput = UnityEngine.Input;
using SynthesisAPI.PreferenceManager;

#nullable enable

namespace SynthesisAPI.InputManager
{
    public static class InputManager // TODO make static class wrapper around singleton
    {
        private static readonly string ModuleName = "API"; // TODO

        // Map for binding DigitalInput to EventHandlers
        private static BiDictionary<IDigitalInput, string> _mappedDigital = new BiDictionary<IDigitalInput, string>();
        // Used for giving custom names to axes
        private static Dictionary<string, IAxisInput> _mappedAxes = new Dictionary<string, IAxisInput>();

        // Used to identify controller names and type because ps4 be wack
        public static ControllerInfo[] ControllerRegistry = new ControllerInfo[12];

        static InputManager()
        {
            // So we can start detecting changes in controllers;
            var names = UnityInput.GetJoystickNames();
            for (var i = 0; i < ControllerInfo.MaxControllers; i++)
            {
                ControllerRegistry[i] = new ControllerInfo(names.Length > i ? names[i] : "", ControllerType.Other); // Default all the controllers to other
            }
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
            _mappedDigital = PM.GetPreference<BiDictionary<IDigitalInput, string>>(ModuleName, "all_bindings", useJsonDeserialization: true);
        }

        private static void PreSave()
        {
            PM.SetPreference(ModuleName, "all_bindings", _mappedDigital);
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

        /// <summary>
        /// Evaluates the type of all the controllers. There isn't an effective way
        /// of isolating this process to just newly connected/disconnected controllers
        /// so this will just be run when it we detect any change in controllers.
        /// </summary>
        public static void EvaluateControllerTypes()
        {
            for (int joy = 0; joy < ControllerInfo.MaxControllers; joy++)
            {
                int joy_index = joy + 1;
                ControllerType prev = ControllerRegistry[joy].Type;
                if (UnityInput.GetAxis("Joystick " + joy_index + " Axis 4") < -0.9
                    && UnityInput.GetAxis("Joystick " + joy_index + " Axis 5") < -0.9)
                {
                    ControllerRegistry[joy].Type = ControllerType.Ps4;
                    Debug.Log(joy + " is Ps4");
                } 
                else
                {
                    ControllerRegistry[joy].Type = ControllerType.Other;
                }
                if (ControllerRegistry[joy].Type != prev)
                {
                    EventBus.EventBus.Push(new ControllerStatusEvent(joy, ControllerRegistry[joy].Name, ControllerRegistry[joy].Type)); // TODO either rename event or add more types to ControllerType
                }
            }
        }

        #endregion

        public static void UpdateDigitalStates()
        {
            DigitalState inputState;
            foreach (var kvp in InputManager._mappedDigital)
            {
                inputState = kvp.Key.GetState();
                if (inputState != DigitalState.None)
                {
                    EventBus.EventBus.Push($"input/{kvp.Value}",
                        new DigitalStateEvent(kvp.Value, inputState));
                }
            }
        }

        /// <summary>
        /// Checks for controller change, and if so, re-evaluates which controllers
        /// are ps4 and which aren't.
        /// </summary>
        public static void UpdateControllerTypes()
        {
            int res = InputManager.ControllerRegistry.Length;
            string[] currentNames = UnityInput.GetJoystickNames();
            for (int i = 0; i < ControllerInfo.MaxControllers; i++)
            {
                var name = currentNames.Length > i ? currentNames[i] : "";
                if (name.Equals(InputManager.ControllerRegistry[i].Name))
                    res -= 1;
                InputManager.ControllerRegistry[i].Name = name;
            }
            if (res != 0)
            {
                InputManager.EvaluateControllerTypes();
            }
        }
    }
    
    
}