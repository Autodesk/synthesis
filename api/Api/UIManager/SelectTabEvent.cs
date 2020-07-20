using SynthesisAPI.EventBus;

namespace SynthesisAPI.UIManager
{
    public class SelectTabEvent: IEvent
    {
        public string TabName;

        public SelectTabEvent(string tabName)
        {
            TabName = tabName;
        }
    }
}