using System;

namespace SynthesisAPI.Modules.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ModuleExportAttribute : Attribute
	{
		public readonly string? ParentModule;


		public ModuleExportAttribute() {}

		public ModuleExportAttribute(string parentModule)
		{
			ParentModule = parentModule;
		}
	}
}