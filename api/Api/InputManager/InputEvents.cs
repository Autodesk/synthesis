using System;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.EventBus;

namespace SynthesisAPI.InputManager.InputEvents
{
    public interface InputEvent : IEvent
    {
        string Name { get; }
    }
    public class AnalogEvent : InputEvent
    {
        public string Name { get; private set; }
        public float Value { get; private set; }
        public AnalogEvent(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }
    public class DigitalEvent : InputEvent
    {
        public string Name { get; private set; }
        public DigitalState State { get; private set; }
        public DigitalEvent(string name, DigitalState state)
        {
            Name = name;
            State = state;
        }
    }
}
