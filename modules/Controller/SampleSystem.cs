using SynthesisAPI.EventBus;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Runtime;

namespace Controller
{
	[ModuleExport]
	public class SampleSystem : SystemBase
	{
		private int _counter;
		private int _counter2;

		private bool _doOnce = true;

		public override void OnUpdate()
		{
			if (_doOnce)
			{
				var asset = AssetManager.GetAsset<JsonAsset>("/modules/controller/test.json");
				ApiProvider.Log(asset.Name + " " + asset.ReadToEnd());
				_doOnce = false;
			}

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