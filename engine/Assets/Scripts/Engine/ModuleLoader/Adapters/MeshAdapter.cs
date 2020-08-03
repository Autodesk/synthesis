using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
using UnityEngine;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh>
	{
		public void Awake()
		{
			if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
				filter = gameObject.AddComponent<MeshFilter>();
			if (gameObject.GetComponent<MeshRenderer>() == null)
				gameObject.AddComponent<MeshRenderer>();
		}

		public void Update()
		{
			if (instance.Changed)
			{
				ToUnity();
				instance.ProcessedChanges();
			}
		}

		private void ToUnity()
        {
			filter.mesh.vertices = Convert(instance.Vertices);
			filter.mesh.uv = Convert(instance.UVs);
			filter.mesh.triangles = instance.Triangles.ToArray();
		}

		public void SetInstance(Mesh mesh)
		{
			instance = mesh;
			ToUnity();
		}

		public static Mesh NewInstance()
		{
			return new Mesh();
		}

		private Mesh instance;
		private MeshFilter filter;

		private Vector3[] Convert(List<Vector3D> vec)
		{
			Vector3[] vectors = new Vector3[vec.Count];
			for (int i = 0; i < vec.Count; i++)
				vectors[i] = MathUtil.MapVector3D(vec[i]);
			return vectors;
		}
		private Vector2[] Convert(List<Vector2D> vec)
		{
			Vector2[] vectors = new Vector2[vec.Count];
			for (int i = 0; i < vec.Count; i++)
				vectors[i] = MathUtil.MapVector2D(vec[i]);
			return vectors;
		}
	}
}