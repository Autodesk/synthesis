using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.InputManager.Axis;

namespace SynthesisAPI.InputManager.Digital
{
    /// <summary>
    /// For keyboard input
    /// </summary>
    public class KeyDigital : IDigitalInput, IAxisInput
    {
        public KeyCode[] keys { get; private set; }

        public static implicit operator KeyCode[](KeyDigital i) => i.keys;
        public static implicit operator KeyDigital(KeyCode[] ks) => new KeyDigital(ks);
        public static implicit operator KeyDigital(KeyCode k) => new KeyDigital(k);
        public static implicit operator int[](KeyDigital i) => i.keys.ToIntArray();

        public static explicit operator KeyDigital(string[] keyStrings) => FromStringArray(keyStrings);
        public static explicit operator string[](KeyDigital data) => data.ToStringArray();

        public int Length { get => keys.Length; }

        private KeyDigital(params KeyCode[] ks)
        {
            this.keys = ks;
        }

        #region Getting Key States

        public IDigitalInput.DigitalState GetState()
        {
            bool getAtleastOneDown = false;
            int up = keys.Length;
            int down = keys.Length;
            foreach (KeyCode k in keys)
            {
                // Skip over this input
                if (k.ToString().ToLower().StartsWith("joystick"))
                {
                    up -= 1;
                    down -= 1;
                    continue;
                }
                if (UnityEngine.Input.GetKey(k))
                {
                    down -= 1;
                    if (UnityEngine.Input.GetKeyDown(k))
                    {
                        getAtleastOneDown = true;
                    }
                } else if (UnityEngine.Input.GetKeyUp(k))
                {
                    up -= 1;
                }
            }
            if (down == 0)
            {
                if (getAtleastOneDown)
                    return IDigitalInput.DigitalState.Down;
                else
                    return IDigitalInput.DigitalState.Held;
            } else if (up == 0)
            {
                return IDigitalInput.DigitalState.Up;
            } else
            {
                return IDigitalInput.DigitalState.None;
            }
        }

        /*
        public bool GetHeld() {
            int result = keys.Length;
            foreach (KeyCode k in keys)
            {
                if (k.ToString().ToLower().StartsWith("joystick"))
                {
                    result -= 1;
                    continue;
                }
                if (UnityEngine.Input.GetKey(k)) result -= 1;
            }
            return result == 0;
        }

        public bool GetDown()
        {
            bool getAtleastOneDown = false;
            int result = keys.Length;
            foreach (KeyCode k in keys)
            {
                if (k.ToString().ToLower().StartsWith("joystick"))
                {
                    result -= 1;
                    continue;
                }
                if (UnityEngine.Input.GetKey(k))
                {
                    result -= 1;
                    if (UnityEngine.Input.GetKeyDown(k))
                    {
                        getAtleastOneDown = true;
                    }
                }
            }
            return result == 0 && getAtleastOneDown;
        }

        public bool GetUp()
        {
            int result = keys.Length;
            foreach (KeyCode k in keys)
            {
                if (k.ToString().ToLower().StartsWith("joystick"))
                {
                    result -= 1;
                    continue;
                }
                if (UnityEngine.Input.GetKeyUp(k)) result -= 1;
            }
            return result == 0;
        }
        */

        public float GetValue(bool positiveOnly = false)
        {
            var state = GetState();
            return state == IDigitalInput.DigitalState.Held ||
                state == IDigitalInput.DigitalState.Down ? 1 : 0;
        }

        public static KeyDigital GetCurrentlyActiveKeyDigital(params KeyCode[] keysToIgnore)
        {
            List<KeyCode> detectedKeys = new List<KeyCode>();
            foreach (var val in Enum.GetValues(typeof(KeyCode)))
            {
                if (Array.Exists(keysToIgnore, x => x == (KeyCode)val)) continue;

                if (((KeyCode)val).ToString().ToLower().StartsWith("joystick"))
                {
                    continue;
                }

                if (UnityEngine.Input.GetKey((KeyCode)val)) detectedKeys.Add((KeyCode)val);
            }

            return detectedKeys.ToArray();
        }

        #endregion

        public override int GetHashCode()
        {
            int a = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                a += (i + 1) * keys[i].ToInt();
            }
            return a;
        }

        public override string ToString()
        {
            if (keys.Length == 0) return "null";

            string a = keys[0].ToString();
            for (int i = 1; i < keys.Length; i++)
            {
                a += ',' + keys[i].ToString();
            }

            return a;
        }

        public string[] ToStringArray()
        {
            List<string> keyStrings = new List<string>();

            foreach (KeyCode k in keys)
            {
                keyStrings.Add(k.ToString());
            }

            return keyStrings.ToArray();
        }

        public static KeyDigital FromStringArray(string[] keyStrings)
        {
            List<KeyCode> keyCodes = new List<KeyCode>();

            KeyCode temp;
            foreach (string k in keyStrings)
            {
                if (!Enum.TryParse(k, out temp)) throw new ArgumentException(string.Format("Failed to read \"{0}\" as a KeyCode", k));
                keyCodes.Add(temp);
            }
            return keyCodes.ToArray();
        }

        public override bool Equals(object obj)
        {
            try
            {
                KeyDigital compare = (KeyDigital)obj;
                foreach (KeyCode k in compare.keys)
                {
                    if (!Array.Exists(keys, x => x == k)) return false;
                }
                return compare.Length == Length;
            } catch { return false; }
        }
    }
}