using System;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.EventBus;

namespace SynthesisAPI.InputManager.InputEvents
{
    public interface IInputEvent : IEvent
    {
        string Name { get; }
    }
    /*
    public class AnalogEvent : IInputEvent // TODO make an event stream type instead
    {
        public string Name { get; private set; }
        public float Value { get; private set; }
        public AnalogEvent(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }
    */
    public class DigitalEvent : IInputEvent
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
