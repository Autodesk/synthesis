using SynthesisAPI.EventBus;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;

namespace Controller
{
	[ModuleExport]
	public class SampleSystem : SystemBase
	{
		private int _counter;
		private int _counter2;
		public override void OnUpdate()
		{
			_counter++;
			if (_counter == 1000)
			{
				EventBus.Push(new SampleEvent("test", _counter2));
				_counter2++;
				_counter = 0;
			}
		}

		public override void OnPhysicsUpdate() { }
	}
}