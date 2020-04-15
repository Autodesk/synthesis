using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator.Input.InputTypes {
    public class KeyInput : InputType
    {
        public (KeyCode modifier, KeyCode key) keyCodeInfo { get; private set; }

        public delegate void KeyUpdate();
        public KeyUpdate KeyDown, KeyUp, KeyHeld;

        public KeyInput(KeyCode mod, KeyCode code, Action keyDown, Action keyUp, Action keyHeld)
        {
            keyCodeInfo = (mod, code);

            KeyDown += () => keyDown();
            KeyUp += () => keyUp();
            keyHeld += () => keyHeld();
        }

        public bool GetKeyHeld() { return UnityEngine.Input.GetKey(keyCodeInfo.key); }
        public bool GetKeyDown() { return UnityEngine.Input.GetKeyDown(keyCodeInfo.key); }
        public bool GetKeyUp() { return UnityEngine.Input.GetKeyUp(keyCodeInfo.key); }

        public void ProcessKey()
        {
            if (GetKeyHeld()) KeyHeld();
            if (GetKeyDown()) KeyDown();
            if (GetKeyUp()) KeyUp();
        }

        public static (KeyCode mod, KeyCode key) GetCurrentlyActiveKey()
        {
            (KeyCode mod, KeyCode key) res = (KeyCode.None, KeyCode.None);

            foreach (var val in Enum.GetValues(typeof(KeyCode)))
            {
                KeyCode key = (KeyCode)val;

                if (
                    key == KeyCode.LeftAlt ||
                    key == KeyCode.RightAlt ||
                    key == KeyCode.LeftControl ||
                    key == KeyCode.RightControl ||
                    key == KeyCode.LeftCommand ||
                    key == KeyCode.RightCommand ||
                    key == KeyCode.LeftShift ||
                    key == KeyCode.RightShift
                    )
                {
                    res.mod = key;
                }
                else if (UnityEngine.Input.GetKey(key))
                {
                    // Debug.Log(key.ToString());
                    res.key = key;
                }
            }

            return res;
        }
    }
}