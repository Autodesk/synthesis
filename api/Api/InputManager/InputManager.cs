using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using Input = SynthesisAPI.InputManager.Inputs.Input;

namespace SynthesisAPI.InputManager
{
    public static class InputManager
    {
        internal static Dictionary<string, Digital[]> _mappedDigitalInputs = new Dictionary<string, Digital[]>();
        internal static Dictionary<string, Analog> _mappedValueInputs = new Dictionary<string, Analog>();
        public static IReadOnlyDictionary<string, Digital[]> MappedDigitalInputs {
            get => new ReadOnlyDictionary<string, Digital[]>(_mappedDigitalInputs);
        }
        public static IReadOnlyDictionary<string, Analog> MappedValueInputs {
            get => new ReadOnlyDictionary<string, Analog>(_mappedValueInputs);
        }

        public static void AssignDigitalInput(string name, Digital input, EventBus.EventBus.EventCallback callback = null) // TODO remove callback argument?
        {
            _mappedDigitalInputs[name] = new Digital[] { input };
            if (callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void UnassignDigitalInput(string name)
        {
            _mappedDigitalInputs.Remove(name);
            EventBus.EventBus.RemoveAllTagListeners($"input/{name}");
        }

        public static void AssignDigitalInputs(string name, Digital[] input, EventBus.EventBus.EventCallback callback = null)
        {
            _mappedDigitalInputs[name] = input;
            if(callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void AssignValueInput(string name, Analog input)
        {
            _mappedValueInputs[name] = input;
        }

        public static void UpdateInputs()
        {
            foreach(string name in _mappedDigitalInputs.Keys)
            {
                foreach(Input input in _mappedDigitalInputs[name])
                {
                    if (!input.Name.EndsWith("non-ui") && input.Update())
                    {
                        if (input is MouseDown mouseDown)
                        {
                            EventBus.EventBus.Push($"input/{name}",
                                new MouseDownEvent(name, mouseDown.Value, mouseDown.State, mouseDown.MousePosition)
                                );
                        }
                        else if (input is Digital digitalInput)
                        {
                            EventBus.EventBus.Push($"input/{name}",
                                new DigitalEvent(name, digitalInput.Value, digitalInput.State)
                                );
                        }
                    }
                }
            }
        }

        public static float GetValue(string name)
        {
            if (_mappedValueInputs.ContainsKey(name))
            {
                _mappedValueInputs[name].Update();
                return _mappedValueInputs[name].Value;
            }
            throw new Exception($"Value Input is not mapped with name \"{name}\"");
        }

        public static void SetAllDigitalInputs(Dictionary<string, Digital[]> input)
        {
            _mappedDigitalInputs = input;
        }

        public static void SetAllValueInputs(Dictionary<string, Analog> input) {
            _mappedValueInputs = input;
        }
    }
}
