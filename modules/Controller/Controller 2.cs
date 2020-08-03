using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;

namespace Controller
{
    [ModuleExport]
	public class Controller
	{
		[Callback]
		public void SampleListenCallback(SampleEvent s)
		{
			SynthesisAPI.Utilities.Logger.Log(s.A);
			SynthesisAPI.Utilities.Logger.Log(s.B);
		}
	}
}