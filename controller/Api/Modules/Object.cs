using System;
using System.Collections.Generic;
using SynthesisAPI.Runtime;
using UnityEngine;

namespace SynthesisAPI.Modules
{
	public abstract class Object
	{

		public override int GetHashCode() => _id.GetHashCode();

		private Guid _id;
		public Transform Transform => ApiProvider.GetTransformById(_id)!;

		public Object()
		{
			Instantiate();
		}

		public Guid GetId() => _id;

		private void Instantiate()
		{
			ApiProvider.Instantiate(this, ref _id);
		}

		public Component AddComponent(Type t) => ApiProvider.AddComponent(t, _id)!;
		public Component AddComponent<TComponent>() where TComponent : Component =>
			ApiProvider.AddComponent<TComponent>(_id)!;

		public List<Component> GetComponents() => ApiProvider.GetComponents(_id);
		public List<TComponent> GetComponents<TComponent>() where TComponent : Component =>
			ApiProvider.GetComponents<TComponent>(_id);

		private bool Equals(Object other)
		{
			return _id.Equals(other._id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Object) obj);
		}

		public static bool operator==(Object lhs, Object rhs)
        {
			return lhs?._id == rhs?._id;
        }
		public static bool operator!=(Object lhs, Object rhs)
		{
			return lhs?._id != rhs?._id;
		}

		public static implicit operator Object?(Guid objectId) => ApiProvider.GetObjectById(objectId);
	}
}