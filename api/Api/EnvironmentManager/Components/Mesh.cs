using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
	public class Mesh : Component
	{
		public Mesh() { }

		internal Mesh(UnityEngine.Mesh mesh)
		{
			Triangles = new List<int>(mesh.triangles);

			foreach (var v in mesh.vertices)
				Vertices.Add(v.Map());
			foreach (var u in mesh.uv)
				UVs.Add(u.Map());
		}

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

		internal static Mesh FromUnity(UnityEngine.Mesh mesh) => new Mesh(mesh);
		internal UnityEngine.Mesh ToUnity()
		{
			UnityEngine.Mesh m = new UnityEngine.Mesh();
			var verts = new UnityEngine.Vector3[Vertices.Count];
			var uvs = new UnityEngine.Vector2[UVs.Count];
			var tris = Triangles.ToArray();

			for (int i = 0; i < verts.Length; i++)
				verts[i] = Vertices[i].Map();
			for (int i = 0; i < uvs.Length; i++)
				uvs[i] = UVs[i].Map();

			m.vertices = verts;
			m.uv = uvs;
			m.triangles = tris;

			return m;
		}

		public bool Changed { get; private set; }
		internal void ProcessedChanges() => Changed = false;
	}
}