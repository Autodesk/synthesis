
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
			var (str, num) = ParamsHelper.PackParams<string, int>(s.GetArguments());
			ApiProvider.Log(str);
			ApiProvider.Log(num);
		}
	}
}