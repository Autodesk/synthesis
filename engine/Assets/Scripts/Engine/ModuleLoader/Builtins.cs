using System.Collections.Generic;
using Engine.ModuleLoader;
using UnityEngine;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;
namespace Core.ModuleLoader
{
	public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh>
	{
		private void Awake()
		{
			if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
				filter = gameObject.AddComponent<MeshFilter>();
			if (gameObject.GetComponent<MeshRenderer>() == null)
				gameObject.AddComponent<MeshRenderer>();
		}

		private void Update()
		{
			if (instance.DidChange)
			{
				filter.mesh.vertices = Convert(instance.Vertices);
				filter.mesh.uv = Convert(instance.UVs);
				filter.mesh.triangles = instance.Triangles.ToArray();
				instance.ProcessedChanges();
			}
		}

		public void SetInstance(Mesh mesh)
		{
			instance = mesh;
		}

		public static Mesh NewInstance()
		{
			return new Mesh();
		}

		private Mesh instance;
		private MeshFilter filter;

		private Vector3[] Convert(List<SynthesisAPI.Utilities.Vector3> vec)
        {
			Vector3[] vectors = new Vector3[vec.Count];
			for(int i = 0; i < vec.Count; i++)
				vectors[i] = vec[i];
			return vectors;
        }
		private Vector2[] Convert(List<SynthesisAPI.Utilities.Vector2> vec)
		{
			Vector2[] vectors = new Vector2[vec.Count];
			for (int i = 0; i < vec.Count; i++)
				vectors[i] = vec[i];
			return vectors;
		}
	}
}