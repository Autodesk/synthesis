using SynthesisAPI.EventBus;

namespace Controller
{
	public class SampleEvent : IEvent
	{
		public string A { get; private set; }
		public int B { get; private set; }

		public SampleEvent(string a, int b)
		{
			A = a;
			B = b;
		}
	}
}