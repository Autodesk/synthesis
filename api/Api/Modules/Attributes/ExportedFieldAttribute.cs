using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ExportedFieldAttribute : Attribute
	{
		public ExportedFieldAttribute(string name)
		{
			Name = name;
		}

		public string? Name;

	}
}