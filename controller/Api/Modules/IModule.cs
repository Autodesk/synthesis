using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SynthesisAPI.Modules.Attributes;

namespace SynthesisAPI.Modules
{
	public interface IModule
	{
		public string Version();
	}

	// ReSharper disable once InconsistentNaming
	public static class IModuleExtensions
	{
		public static List<Type>? ExportedTypes(Type t)
		{
			if (!(typeof(IModule).IsAssignableFrom(t)))
			{
				return null;
			}
			if (t.Assembly.GetTypes().Where(type => typeof(IModule).IsAssignableFrom(type)).ToList().Count > 1)
			{
				var defaultCount = t.Assembly.GetTypes()
					.Count(type => type.GetCustomAttribute(typeof(DefaultModuleAttribute)) != null);
				if (defaultCount > 1)
				{
					throw new Exception("Multiple default modules defined");
				} else if (defaultCount == 0)
				{
					throw new Exception("No default modules where found");
				}
				else
				{
					if (t.GetCustomAttribute(typeof(DefaultMemberAttribute)) != null)
					{
						return t.Assembly.GetTypes()
							.Where((type, i) =>
								type.GetCustomAttribute(typeof(ModuleExportAttribute)) != null &&
								((ModuleExportAttribute) type.GetCustomAttribute(typeof(ModuleExportAttribute)))
								.ParentModule ==
								null || ((ModuleExportAttribute) type.GetCustomAttribute(typeof(ModuleExportAttribute)))
								.ParentModule == t.Name).ToList();
					}
					else
					{
						return t.Assembly.GetTypes()
							.Where((type, i) =>
								type.GetCustomAttribute(typeof(ModuleExportAttribute)) != null &&
								((ModuleExportAttribute) type.GetCustomAttribute(typeof(ModuleExportAttribute)))
								.ParentModule == t.Name).ToList();
					}
				}
			}

			return t.Assembly.GetTypes()
				.Where((type, i) => type.GetCustomAttribute(typeof(ModuleExportAttribute)) != null).ToList();
		}
	}
}