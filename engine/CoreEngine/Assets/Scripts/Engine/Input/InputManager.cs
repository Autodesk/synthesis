using System;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Util;
using UnityInput = UnityEngine.Input;

namespace Synthesis.Simulator.Input
{
    /**
     * Still very much a work in progress
     */
    public static class InputManager
    {
        // Map for binding DigitalInput to EventHandlers
        public static SynList<IDigitalInput, string> MappedDigital = new SynList<IDigitalInput, string>();
        // Used for giving custom names to axes
        public static Dictionary<string, IAxisInput> MappedAxes = new Dictionary<string, IAxisInput>();

        // Used to identify controller type because ps4 be wack
        public static Dictionary<int, ControllerType> ControllerRegistry = new Dictionary<int, ControllerType>();
        public static string[] LastControllerNames = new string[1];

        static InputManager()
        {
            UnityHandler.OnUpdate += Update; // Add the update function to the update event
            UnityHandler.OnFixedUpdate += FixedUpdate;

            for (int i = 1; i <= 11; i++)
            {
                ControllerRegistry.Add(i, ControllerType.Other); // Default all the controllers to other
            }

            // So we can start detecting changes in controllers;
            LastControllerNames = UnityInput.GetJoystickNames();
            EvaluateControllerTypes();

            // Assign general axes
            MappedAxes["MouseX"] = (DualAxis)"Mouse X";
            MappedAxes["MouseY"] = (DualAxis)"Mouse Y";
        }

        private static void Update()
        {
            IDigitalInput.DigitalState inputState;
            foreach (var kvp in MappedDigital)
            {
                inputState = kvp.Key.GetState();
                if (inputState != IDigitalInput.DigitalState.None)
                {
                    // Publish to event bus with value identifier and state
                }
            }
        }

        private static void FixedUpdate()
        {
            int res = LastControllerNames.Length;
            string[] currentNames = UnityInput.GetJoystickNames();
            if (res == currentNames.Length)
            {
                for (int i = 0; i < currentNames.Length; i++)
                {
                    if (currentNames[i].Equals(LastControllerNames[i])) res -= 1;
                }
            }
            if (res != 0)
            {
                LastControllerNames = currentNames;
                EvaluateControllerTypes();
            }
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
    }
}