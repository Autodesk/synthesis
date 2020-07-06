using SynthesisAPI.EventBus;

namespace Controller
{
	public class SampleEvent : IEvent
	{
		private string a;
		private int b;

		public SampleEvent(string a, int b)
		{
			this.a = a;
			this.b = b;
		}

		public object[] GetArguments() => new object[] {a,b};
	}
}