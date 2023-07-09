using SynthesisAPI.EventBus;

namespace Engine.ModuleLoader {
    public class LoadModuleEvent : IEvent {
        public LoadModuleEvent(string name, string version) {
            Name    = name;
            Version = version;
        }

        public readonly string Name;
        public readonly string Version;

        public object[] GetArguments() => new object[] { Name, Version };
    }
}