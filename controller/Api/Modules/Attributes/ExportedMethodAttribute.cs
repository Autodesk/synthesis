using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ExportedMethodAttribute : Attribute
	{
		public ExportedMethodAttribute(string name)
		{
			Name = name;
		}

		public string? Name;
	}
}