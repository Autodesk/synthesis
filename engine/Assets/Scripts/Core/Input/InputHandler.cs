using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Synthesis.Simulator.Input.InputTypes;

namespace Synthesis.Simulator.Input
{
    /**
     * Still very much a work in progress
     */
    public class InputHandler : MonoBehaviour
    {
        public InputHandler Instance { get; private set; }

        private List<KeyInput> boundKeys = new List<KeyInput>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            boundKeys.ForEach(x => x.ProcessKey());
        }

        public KeyBindingResult BindKeyInput(KeyCode modifier, KeyCode key, Action keyDown = null, Action keyUp = null, Action keyHeld = null)
        {
            // KeyInput i = new KeyInput(KeyCode.Space, keyUp: () => Debug.Log("Spacebar is Up"));

            // Process to bind a key

            /**
             * 1 => Store if keybind exists already
             * 2 => Create the keybind (Adapt if actions are null) 
             * 3 => Delete any pre-existing binding, then store it in bound keys
             * 4 => If binding was overriden, return OverridKey, else return Ok
             */

            // 1
            bool exists = boundKeys.Exists(x => (x.keyCodeInfo.key == key) && (x.keyCodeInfo.modifier == modifier));

            // 2
            if (keyDown == null) keyDown = () => { };
            if (keyDown == null) keyUp = () => { };
            if (keyDown == null) keyHeld = () => { };
            KeyInput input = new KeyInput(modifier, key, keyDown, keyUp, keyHeld);

            // 3
            if (exists) boundKeys.Remove(boundKeys.Find(x => x.keyCodeInfo.key == key));
            boundKeys.Add(input);

            // 4
            if (exists) return KeyBindingResult.OverridKey;
            else return KeyBindingResult.Ok;
        }

        public enum KeyBindingResult
        {
            Ok, OverridKey, Error
        }
    }
}