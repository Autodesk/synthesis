using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
	public class TaggedCallbackAttribute : Attribute
	{
		public readonly string Tag;

		public TaggedCallbackAttribute(string tag) {
			Tag = tag;
		}
	}
}