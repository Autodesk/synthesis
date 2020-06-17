using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ComponentAttribute : Attribute
	{
		public readonly Type Type;
		public ComponentAttribute(Type type)
		{
			Type = type;
		}
	}
}