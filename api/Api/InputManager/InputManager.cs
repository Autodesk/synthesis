using System;
using System.Collections.Generic;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Runtime;

namespace SynthesisAPI.InputManager
{
    public static class InputManager
    {
        static Dictionary<string, Input[]> _mappedInputs = new Dictionary<string, Input[]>();

        public static void AssignInputsToEvent(string name, Input input, EventBus.EventBus.EventCallback callback = null)
        {
            _mappedInputs[name] = new Input[] { input };
            if (callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void AssignInputsToEvent(string name, Input[] input, EventBus.EventBus.EventCallback callback = null)
        {
            _mappedInputs[name] = input;
            if(callback != null)
                EventBus.EventBus.NewTagListener($"input/{name}", callback);
        }

        public static void UpdateInputs()
        {
            foreach(string name in _mappedInputs.Keys)
            {
                foreach(Input input in _mappedInputs[name])
                {
                    if(input.Update())
                    {
                        if (input is Digital)
                            EventBus.EventBus.Push($"input/{name}", new DigitalEvent(input.Name, ((Digital)input).State));
                        else
                            EventBus.EventBus.Push($"input/{name}", new AnalogEvent(input.Name, ((Analog)input).Value));
                    }
                }
            }
        }

        public static void SetAllInputs(Dictionary<string, Input[]> input)
        {
            _mappedInputs = input;
        }

        public static Dictionary<string, Input[]> GetAllInputs()
        {
            return _mappedInputs;
        }
    }
}
