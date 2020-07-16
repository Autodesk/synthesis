using SynthesisAPI.EventBus;

namespace SynthesisAPI.UIManager
{
    public struct SelectTabEvent: IEvent
    {
        public string TabName;

        public SelectTabEvent(string tabName)
        {
            TabName = tabName;
        }

        public object[] GetArguments() => new object[] {TabName};
    }
}