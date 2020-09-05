using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager.UIComponents;

namespace SynthesisAPI.UIManager
{
    public class ClosePanelEvent : IEvent
    {
        public Panel Panel;

        public ClosePanelEvent(Panel panel)
        {
            Panel = panel;
        }
    }
}