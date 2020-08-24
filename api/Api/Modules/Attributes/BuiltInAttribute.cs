using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class BuiltInAttribute : Attribute
	{
		public BuiltInAttribute(Type t)
		{
			DependencyType = t;
		}
		public Type DependencyType = null;
	}
}