using System.Collections.Generic;
using SynthesisAPI.Modules.Attributes;
using UnityEngine;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
	public class Mesh : Component
	{
		private List<Vector3> _vertices = new List<Vector3>();
		private List<Vector2> _uvs = new List<Vector2>();
		private List<int> _triangles = new List<int>();


		public List<Vector3> Vertices
		{
			get => _vertices;
			set {
				_vertices = value;
			    DidChange = true;
			}
		}
		public List<Vector2> UVs
		{
			get => _uvs;
			set {
				_uvs = value;
			    DidChange = true;
			}
		}

		public List<int> Triangles
		{
			get => _triangles;
			set {
				_triangles = value;
			    DidChange = true;
			}
		}

		public void ProcessedChanges() => DidChange = false;

		public bool DidChange { get; private set; }
	}
}