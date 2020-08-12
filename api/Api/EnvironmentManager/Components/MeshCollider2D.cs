using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
	public class MeshCollider2D : Component
	{
		public delegate void EventDelegate();

		public Mesh Mesh { get; internal set; }
		public Bounds Bounds { get; internal set; }
		public EventDelegate OnMouseDown;
		public EventDelegate OnMouseUp;


		public MeshCollider2D()
		{
			Mesh = new Mesh();
			Bounds = new Bounds();
			OnMouseDown = () => { };
			OnMouseUp = () => { };
		}

		public bool Changed { get; internal set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}