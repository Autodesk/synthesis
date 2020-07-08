using System;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.Digital;

namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for when a registered key press is activated
    /// </summary>
    public class DigitalStateEvent : IEvent
    {
        public string Name { get; private set; } // Rename to InputName or something
        public InputManager.DigitalState KeyState { get; private set; }

        public DigitalStateEvent(string name, InputManager.DigitalState keyState)
        {
            Name = name;
            KeyState = keyState;
        }

        public object[] GetArguments() => new object[] { Name, KeyState };
    }
}
