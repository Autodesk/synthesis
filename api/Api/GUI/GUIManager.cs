using System;
using System.Collections;
using System.Collections.Generic;

namespace Api.GUI
{
	public class GUIManager
	{
		protected List<GUIInstance> _instances;

		protected Dictionary<Type, IGUIBuilderFactory> _builders;

		internal void RegisterBuilder<TGUI>(IGUIBuilderFactory builderFactory) where TGUI : GUIInstance
		{
			if (_builders.ContainsKey(typeof(TGUI)))
			{
				throw new Exception("builder with this name already exists");
			}

			_builders[typeof(TGUI)] = builderFactory;
		}

		internal void RegisterInstance(GUIInstance i)
		{
			i._id = _instances.Count;
			_instances.Add(i);
		}

		internal void Free(int id)
		{
			if (id > _instances.Count)
			{
				throw new Exception("invalid id");
			}
			if (_instances[id].IsDisposed)
			{
				throw new Exception("Object already freed");
			}
		}

		public IGUIBuilder NewBuilder<TGUI>() where TGUI : GUIInstance => _builders[typeof(TGUI)].New();
	}
}