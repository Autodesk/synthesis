using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator.Input
{
    /// <summary>
    /// For buttons on a gamepad
    /// </summary>
    public class ButtonDigital : IDigitalInput, IAxisInput
    {
        private (int joystick, int button)[] buttons;

        public static implicit operator (int joystick, int button)[](ButtonDigital b) => b.buttons;
        public static implicit operator ButtonDigital((int joystick, int button) info) => new ButtonDigital(info);
        public static implicit operator ButtonDigital((int joystick, int button)[] info) => new ButtonDigital(info);

        public int Length { get => buttons.Length; }

        private ButtonDigital(params (int joystick, int button)[] bs)
        {
            this.buttons = bs;
        }

        #region Getting States

        public bool GetHeld()
        {
            int result = buttons.Length;
            foreach (string buttonName in buttons.ToButtonNameArray())
            {
                if (UnityEngine.Input.GetKey(buttonName)) result -= 1;
            }
            if (GetDown()) return false;
            return result == 0;
        }

        public bool GetDown()
        {
            int result = buttons.Length;
            foreach (string buttonName in buttons.ToButtonNameArray())
            {
                if (UnityEngine.Input.GetKeyDown(buttonName)) result -= 1;
            }
            return result == 0;
        }

        public bool GetUp()
        {
            int result = buttons.Length;
            foreach (string buttonName in buttons.ToButtonNameArray())
            {
                if (UnityEngine.Input.GetKeyUp(buttonName)) result -= 1;
            }
            return result == 0;
        }

        public float GetValue(bool positiveOnly = false)
        {
            return GetDown() || GetHeld() ? 1 : 0;
        }

        public static ButtonDigital GetCurrentlyActiveButtonDigital(params (int joystick, int button)[] buttonsToIgnore)
        {
            List<(int joystick, int button)> buttonsActive = new List<(int joystick, int button)>();
            (int joystick, int button) button;
            for (int j = 1; j < 11; j++)
            {
                for (int b = 1; b < 20; b++)
                {
                    button = (j, b);
                    if (Array.Exists(buttonsToIgnore, x => x == button)) continue;

                    if (UnityEngine.Input.GetKey(button.ToButtonName())) buttonsActive.Add(button);
                }
            }
            return buttonsActive.ToArray();
        }

        public override int GetHashCode()
        {
            int a = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                a *= buttons[i].GetHashCode();
            }
            return a;
        }

        public override bool Equals(object obj)
        {
            try
            {
                ButtonDigital compare = (ButtonDigital)obj;
                foreach (var b in compare.buttons)
                {
                    if (!Array.Exists(buttons, x => x == b)) return false;
                }
                return compare.Length == Length;
            } catch { return false; }
        }

        #endregion

        public static ButtonDigital FromButtonName(string s)
        {
            string[] split = s.Split(' ');
            int j = int.Parse(split[1]);
            int b = int.Parse(split[3]);
            return new ButtonDigital((j, b));
        }
    }
}