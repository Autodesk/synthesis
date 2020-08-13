using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Components
{
	public class Mesh : Component
	{
		private List<Vector3D> _vertices = new List<Vector3D>();
		private List<Vector2D> _uvs = new List<Vector2D>();
		private List<int> _triangles = new List<int>();


		public List<Vector3D> Vertices
		{
			get => _vertices;
			set {
				_vertices = value;
			    Changed = true;
			}
		}
		public List<Vector2D> UVs
		{
			get => _uvs;
			set {
				_uvs = value;
			    Changed = true;
			}
		}

		public List<int> Triangles
		{
			get => _triangles;
			set {
				_triangles = value;
			    Changed = true;
			}
		}

		public bool Changed { get; private set; }
		internal void ProcessedChanges() => Changed = false;
	}
}