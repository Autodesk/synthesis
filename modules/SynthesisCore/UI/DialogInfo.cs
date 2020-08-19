using System;
using SynthesisAPI.EventBus;

namespace SynthesisCore.UI
{
    public struct DialogInfo
    {
        public string Title;
        public string Prompt;
        public string Description;
        public string SubmitButtonText;
        public string CloseButtonText;
        public Action<IEvent> SubmitButtonAction;
        public Action<IEvent> CloseButtonAction;
    }
}