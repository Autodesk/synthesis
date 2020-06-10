using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator.Input
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

        public int Length { get => keys.Length; }

        private KeyDigital(params KeyCode[] ks)
        {
            this.keys = ks;
        }

        #region Getting Key States

        public bool GetHeld() {
            int result = keys.Length;
            foreach (KeyCode k in keys) if (UnityEngine.Input.GetKey(k)) result -= 1;
            return result == 0;
        }

        public bool GetDown()
        {
            int result = keys.Length;
            foreach (KeyCode k in keys) if (UnityEngine.Input.GetKeyDown(k)) result -= 1;
            return result == 0;
        }

        public bool GetUp()
        {
            int result = keys.Length;
            foreach (KeyCode k in keys) if (UnityEngine.Input.GetKeyUp(k)) result -= 1;
            return result == 0;
        }

        public float GetValue(bool positiveOnly = false)
        {
            return GetDown() || GetHeld() ? 1 : 0;
        }

        public static KeyDigital GetCurrentlyActiveKeyDigital(params KeyCode[] keysToIgnore)
        {
            List<KeyCode> detectedKeys = new List<KeyCode>();
            foreach (var val in Enum.GetValues(typeof(KeyCode)))
            {
                if (Array.Exists(keysToIgnore, x => x == (KeyCode)val)) continue;

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
                a += keys[i].ToString();
            }

            return a;
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

    public enum KeyAction
    {
        Up, Down, Held
    }
}