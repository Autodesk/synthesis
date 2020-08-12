using SynthesisAPI.EnvironmentManager;

namespace SynthesisAPI.EnvironmentManager.Components
{
	public class MeshCollider2D : Component
	{
		public Mesh Mesh { get; internal set; }
		public OnClickDelegate OnClick;

		public delegate void OnClickDelegate();

		public MeshCollider2D()
		{
			Mesh = new Mesh();
			OnClick = () => { };
		}


		public bool Changed { get; internal set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}