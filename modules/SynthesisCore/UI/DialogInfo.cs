using System;
using SynthesisAPI.EventBus;

namespace SynthesisCore.UI
{
    public class DialogInfo
    {
        public string Title = "Notification";
        public string Prompt;
        public string Description;
        public string SubmitButtonText = "Submit";
        public string CloseButtonText = "Close";
        public Action<IEvent> SubmitButtonAction;
        public Action<IEvent> CloseButtonAction;
    }
}