using SynthesisAPI.EventBus;

namespace Engine.ModuleLoader
{
	public class LoadModuleEvent : IEvent
	{
		public LoadModuleEvent(string path)
		{
			Path = path;
		}

		public readonly string Path;

		public object[] GetArguments() => new object[]{Path};
	}
}