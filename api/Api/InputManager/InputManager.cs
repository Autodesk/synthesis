using System;
using System.Collections.Generic;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using Input = SynthesisAPI.InputManager.Inputs.Input;

namespace SynthesisAPI.InputManager
{
    public static class InputManager
    {
        internal static Dictionary<string, Digital[]> _mappedDigitalInputs = new Dictionary<string, Digital[]>();
        private static Dictionary<string, Analog> _mappedAxisInputs = new Dictionary<string, Analog>();

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

        public static void AssignAxis(string name, Analog axis)
        {
            _mappedAxisInputs[name] = axis;
        }

        public static void UpdateInputs()
        {
            foreach(string name in _mappedDigitalInputs.Keys)
            {
                foreach(Input input in _mappedDigitalInputs[name])
                {
                    if(!input.Name.EndsWith("non-ui") && input is Digital digitalInput && digitalInput.Update())
                    {
                        EventBus.EventBus.Push($"input/{name}", new DigitalEvent(name, digitalInput.State));
                    }
                }
            }
        }

        public static float GetAxisValue(string name)
        {
            if (_mappedAxisInputs.ContainsKey(name))
            {
                _mappedAxisInputs[name].Update();
                return _mappedAxisInputs[name].Value;
            }
            throw new Exception($"Axis value is not mapped with name \"{name}\"");
        }

        public static void SetAllInputs(Dictionary<string, Digital[]> input)
        {
            _mappedDigitalInputs = input;
        }

        public static Dictionary<string, Digital[]> GetAllInputs()
        {
            return _mappedDigitalInputs;
        }
    }
}
