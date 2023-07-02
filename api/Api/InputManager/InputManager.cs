using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

#nullable enable

using Input = SynthesisAPI.InputManager.Inputs.Input;

namespace SynthesisAPI.InputManager
{
    public static class InputManager
    {
        public static readonly string[] IGNORE_KEYS = new string[] {
            "AltGr"
        };

        // TODO: Should this still be an array?
        internal static Dictionary<string, Digital[]> _mappedDigitalInputs = new Dictionary<string, Digital[]>();
        internal static Dictionary<string, Analog> _mappedValueInputs = new Dictionary<string, Analog>();
        public static IReadOnlyDictionary<string, Digital[]> MappedDigitalInputs {
            get => new ReadOnlyDictionary<string, Digital[]>(_mappedDigitalInputs);
        }
        public static IReadOnlyDictionary<string, Analog> MappedValueInputs {
            get => new ReadOnlyDictionary<string, Analog>(_mappedValueInputs);
        }

        public static void AssignDigitalInput(string name, Digital input, EventBus.EventBus.EventCallback? callback = null) { // TODO remove callback argument?
            _mappedDigitalInputs[name] = new Digital[] { input };
            if (callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void UnassignDigitalInput(string name) {
            if (!_mappedDigitalInputs.ContainsKey(name))
                return;
            _mappedDigitalInputs.Remove(name);
            EventBus.EventBus.RemoveAllTagListeners($"input/{name}");
        }

        public static void AssignDigitalInputs(string name, Digital[] input, EventBus.EventBus.EventCallback? callback = null) {
            _mappedDigitalInputs[name] = input;
            if(callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        /// <summary>
        /// Assign a name to an input.
        /// </summary>
        /// <param name="name">Name of Input</param>
        /// <param name="input">Analog of the input to check for activity for</param>
        /// <param name="mute">Set mute to true if you wish to mute the event call for the input being assigned</param>
        public static void AssignValueInput(string name, Analog input, bool mute = false) {
            _mappedValueInputs[name] = input;
            if (!mute)
                EventBus.EventBus.Push(new ValueInputAssignedEvent(name, input));
        }

        public static void UpdateInputs(uint context = 0x00000001) {
            foreach(string name in _mappedDigitalInputs.Keys) {
                foreach(Input input in _mappedDigitalInputs[name]) {
                    if (!input.Name.EndsWith("non-ui") && input.Update(context: context)) {
                        if (input is MouseDown mouseDown) {
                            EventBus.EventBus.Push($"input/{name}",
                                new MouseDownEvent(name, mouseDown.Value, mouseDown.State, mouseDown.MousePosition)
                                );
                        } else if (input is Digital digitalInput) {
                            EventBus.EventBus.Push($"input/{name}",
                                new DigitalEvent(name, digitalInput.Value, digitalInput.State)
                                );
                        }
                    }
                }
            }
        }

        public static float GetValue(string name) {
            if (_mappedValueInputs.ContainsKey(name)) {
                _mappedValueInputs[name].Update();
                return _mappedValueInputs[name].Value;
            }
            throw new Exception($"Value Input is not mapped with name \"{name}\"");
        }

        public static void SetAllDigitalInputs(Dictionary<string, Digital[]> input) {
            _mappedDigitalInputs = input;
        }

        public static void SetAllValueInputs(Dictionary<string, Analog> input) {
            _mappedValueInputs = input;
        }

        // TODO: Exclusion cases
        public static Analog? GetAny() {
            foreach (var k in AllInputs) {
                if (k.Update(true))
                    return k.WithModifier(GetModifier());
            }

            return null;
        }

        public static int GetModifier() {
            int mod = 0x00;
            ModifierInputs.ForEach(x => {
                x.Update(true);
                if (x.State > DigitalState.None) {
                    mod = mod ^ (int)Enum.Parse(typeof(ModKey), x.Name);
                }
            });
            return mod;
        }

        public static List<Analog> GetAll() => AllInputs.Where(k => k.Update()).ToList();

        private static List<Analog>? _allInputs;
        public static IReadOnlyCollection<Analog> AllInputs {
            get {
                if (_allInputs == null) {
                    _allInputs = new List<Analog>();

                    // KeyCodes
                    var modKeyNames = Enum.GetNames(typeof(ModKey));
                    foreach (var k in Enum.GetNames(typeof(KeyCode))) {
                        if (!modKeyNames.Contains(k) && !IGNORE_KEYS.Contains(k))
                            _allInputs.Add(new Digital(k));
                    }
                    
                    // Joystick Controls
                    for (int j = 0; j <= 11; j++) {
                        for (int ab = 1; ab <= 20; ab++) {
                            var joystickId = j == 0 ? "" : $"{j} ";
                            var joystickPrefix = $"Joystick {joystickId}";
                            _allInputs.Add(new Digital(joystickPrefix + $"Button {ab}"));
                            _allInputs.Add(new Analog(joystickPrefix + $"Axis {ab}", true));
                            _allInputs.Add(new Analog(joystickPrefix + $"Axis {ab}", false));
                        }
                    }
                    
                }
                return _allInputs;
            }
        }

        // constructors need to call this and intialize the list properly, for now this works
        private static List<Digital>? _modifierInputs;
        public static IReadOnlyCollection<Digital> ModifierInputs {
            get {
                if (_modifierInputs == null) {
                    _modifierInputs = new List<Digital>();

                    var modKeys = Enum.GetNames(typeof(ModKey));
                    modKeys.ForEach(x => {
                        _modifierInputs.Add(new Digital(x));
                    });
                }
                return _modifierInputs;
            }
        }
    }

    public class ValueInputAssignedEvent : IEvent {
        public readonly string InputKey;
        public readonly Analog Input;
        public ValueInputAssignedEvent(string key, Analog input) {
            InputKey = key;
            Input = input;
        }
    }
}
