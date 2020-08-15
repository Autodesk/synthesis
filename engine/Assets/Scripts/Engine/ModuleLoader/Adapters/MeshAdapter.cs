using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
using UnityEngine;
using Logger = UnityEngine.Logger;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh>
	{
		private Material _defaultMaterial = null;
		private MeshRenderer _renderer = null;

		private void ToUnity()
        {
			filter.mesh.vertices = Convert(instance.Vertices);
			filter.mesh.uv = Convert(instance.UVs);
			filter.mesh.triangles = instance.Triangles.ToArray();
		}

		public void SetInstance(Mesh mesh)
		{
			instance = mesh;
			
			if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
				filter = gameObject.AddComponent<MeshFilter>();
			if ((_renderer = gameObject.GetComponent<MeshRenderer>()) == null)
				_renderer = gameObject.AddComponent<MeshRenderer>();

			if (_defaultMaterial == null)
			{
				var s = Shader.Find("Universal Render Pipeline/Lit");
				_defaultMaterial = new Material(s);
				_defaultMaterial.color = new Color(0.2f, 0.2f, 0.2f);
				_defaultMaterial.SetFloat("_Smoothness", 0.2f);
			}
			_renderer.material = _defaultMaterial;

			filter.mesh = new UnityEngine.Mesh();
			filter.mesh.vertices = Convert(instance.Vertices);
			filter.mesh.uv = Convert(instance.UVs);
			filter.mesh.triangles = instance.Triangles.ToArray();
			
			instance.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName.ToLower())
				{
					case "vertices":
						filter.mesh.vertices = Convert(instance._vertices);
						break;
					case "uvs":
						filter.mesh.uv = Convert(instance._uvs);
						break;
					case "triangles":
						filter.mesh.triangles = instance._triangles.ToArray();
						break;
					case "color":
						_renderer.material.color = new Color(instance._color.r, instance._color.g, instance._color.b, instance._color.a);
						break;
					default:
						SynthesisAPI.Utilities.Logger.Log("Property not setup", LogLevel.Warning);
						break;
				}
			};
		}

		public static Mesh NewInstance()
		{
			return new Mesh();
		}

		internal Mesh instance;
		internal MeshFilter filter;
		internal MeshRenderer renderer;

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