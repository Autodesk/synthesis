using SynthesisAPI.EventBus;

namespace Engine.ModuleLoader
{
	public class LoadModuleEvent : IEvent
	{
		public LoadModuleEvent(string path)
		{
			_path = path;
		}

		private readonly string _path;

		public object[] GetArguments() => new object[]{_path};
	}
}