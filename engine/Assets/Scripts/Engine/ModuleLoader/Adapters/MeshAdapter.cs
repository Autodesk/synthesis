using System.Collections.Generic;
using Engine.Util;
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

			if ((filter = gameObject.GetComponent<MeshFilter>()) == null)
				filter = gameObject.AddComponent<MeshFilter>();
			if ((renderer = gameObject.GetComponent<MeshRenderer>()) == null)
				renderer = gameObject.AddComponent<MeshRenderer>();

			if (_defaultMaterial == null)
			{
				var s = Shader.Find("Universal Render Pipeline/Lit");
				_defaultMaterial = new Material(s);
				_defaultMaterial.color = new Color(0.2f, 0.2f, 0.2f);
				_defaultMaterial.SetFloat("_Smoothness", 0.2f);
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
			if (instance != null)
			{
				filter.mesh.vertices = ((List<Vector3>)Utilities.MapAll(instance.Vertices, MathUtil.MapVector3D)).ToArray();
				filter.mesh.uv = ((List<Vector2>)Utilities.MapAll(instance.UVs, MathUtil.MapVector2D)).ToArray();
				filter.mesh.triangles = instance.Triangles.ToArray();
			}
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
		private new MeshRenderer renderer;
	}
}