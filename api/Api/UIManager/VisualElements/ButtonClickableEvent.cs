using System;
using SynthesisAPI.EventBus;

namespace SynthesisAPI.UIManager.VisualElements
{
    public struct ButtonClickableEvent: IEvent
    {
        public object[] GetArguments() => null!;
    }
}