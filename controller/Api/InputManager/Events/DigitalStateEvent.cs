using System;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.Digital;

namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for when a registered key press is activated
    /// TODO: Inherit yet to be made Event interface
    /// </summary>
    public class DigitalStateEvent : IEvent
    {
        public string Name { get; private set; }
        public InputManager.DigitalState KeyState { get; private set; }

        public DigitalStateEvent(string name, InputManager.DigitalState keyState)
        {
            Name = name;
            KeyState = keyState;
        }

        public object[] GetArguments() => new object[] { Name, KeyState };
    }
}
