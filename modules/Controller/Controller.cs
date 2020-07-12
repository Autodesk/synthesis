
using System;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;

namespace Controller
{
	[ModuleExport]
	public class Controller
	{
		[Callback]
		public void SampleListenCallback(SampleEvent s)
		{
			ApiProvider.Log(s.A);
			ApiProvider.Log(s.B);
		}
	}
}