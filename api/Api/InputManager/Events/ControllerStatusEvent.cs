using SynthesisAPI.EventBus;

namespace SynthesisAPI.InputManager.Events
{
    /// <summary>
    /// Event for when a controller is connected/disconnected or type is changed
    /// </summary>
    public class ControllerStatusEvent : IEvent
    {
        public int Index { get; private set; }

        public string ControllerName { get; private set; }

        public ControllerType NewType { get; private set; }

        public ControllerStatusEvent(int index, string controllerName, ControllerType newType)
        {
            Index = index;
            ControllerName = controllerName;
            NewType = newType;
        }

        public object[] GetArguments() => new object[] { Index, NewType };
    }
}
