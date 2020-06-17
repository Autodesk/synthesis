using UnityEngine;

namespace SynthesisAPI.Modules
{
	public class Component
	{
		public string? Tag;
		public Object? Parent { get; protected set; }
		public Transform? Transform => Parent?.Transform;

		internal void SetParent(Object parent) => Parent = parent;

	}
}