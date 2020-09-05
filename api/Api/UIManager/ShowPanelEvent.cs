using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager.UIComponents;

namespace SynthesisAPI.UIManager
{
    public class ShowPanelEvent : IEvent
    {
        public Panel Panel;

        public ShowPanelEvent(Panel panel)
        {
            Panel = panel;
        }
    }
}