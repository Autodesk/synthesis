using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
using UnityEngine;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh>
	{
		private static Material _defaultMaterial = null;

		public void Awake()
		{
			MeshRenderer renderer;

			if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
				filter = gameObject.AddComponent<MeshFilter>();
			if ((renderer = gameObject.GetComponent<MeshRenderer>()) == null)
				renderer = gameObject.AddComponent<MeshRenderer>();

			if (_defaultMaterial == null)
			{
				var s = Shader.Find("Universal Render Pipeline/Lit");
				_defaultMaterial = new Material(s);
				_defaultMaterial.color = new Color(1, 0.2f, 0.2f);
				_defaultMaterial.SetFloat("_Smoothness", 0f);
			}
			renderer.material = _defaultMaterial;
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