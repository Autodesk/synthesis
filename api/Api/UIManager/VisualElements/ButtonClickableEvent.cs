using System;
using SynthesisAPI.EventBus;

namespace SynthesisAPI.UIManager.VisualElements
{
    public struct ButtonClickableEvent: IEvent
    {
        public readonly string Name;

        public ButtonClickableEvent(string name)
        {
            Name = name;
        }

        public object[] GetArguments() => new[] { Name };
    }
}