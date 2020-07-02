using System;
using SynthesisAPI.InputManager.Digital;

namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for when a registered key press is activated
    /// TODO: Inherit yet to be made Event interface
    /// </summary>
    public struct DigitalStateEvent
    {
        public string Name { get; private set; }
        public IDigitalInput.DigitalState KeyState { get; private set; }

        public DigitalStateEvent(string name, IDigitalInput.DigitalState keyState)
        {
            Name = name;
            KeyState = keyState;
        }
    }
}
