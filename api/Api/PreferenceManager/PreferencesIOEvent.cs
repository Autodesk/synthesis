using SynthesisAPI.EventBus;

namespace SynthesisAPI.PreferenceManager
{
    public class PreferencesIOEvent : IEvent
    {
        public Status PreferencesStatus { get; private set; }

        public PreferencesIOEvent(Status status)
        {
            PreferencesStatus = status;
        }

        public enum Status
        {
            PreSave = 1, PostLoad = 2
        }
    }
}