using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class InitializationPriorityAttribute  : Attribute
	{

		public InitializationPriorityAttribute(byte v)
		{
			Value = v;
		}

		public InitializationPriorityAttribute()
		{
			Value = 255;
		}
		public readonly byte Value;
	}
}