using SynthesisAPI.EventBus;

namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for when a controller is connected/disconnected or type is changed
    /// </summary>
    public class ControllerConnectionEvent : IEvent
    {
        public int Index { get; private set; }
        public InputManager.ControllerType NewType { get; private set; }

        public ControllerConnectionEvent(int index, InputManager.ControllerType newType)
        {
            Index = index;
            NewType = newType;
        }

        public object[] GetArguments() => new object[] { Index, NewType };
    }
}
