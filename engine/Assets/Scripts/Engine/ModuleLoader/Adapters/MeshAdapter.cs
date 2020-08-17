using System.Collections.Generic;
using System.Linq;
using Engine.Util;
using SynthesisAPI.Utilities;
using UnityEditor;
using UnityEngine;
using Logger = UnityEngine.Logger;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class MeshAdapter : MonoBehaviour, IApiAdapter<Mesh>
	{
		private Material _defaultMaterial = null;
		private MeshRenderer _renderer = null;

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
			filter.mesh.vertices = Misc.MapAll(instance._vertices, Misc.MapVector3D).ToArray();
			filter.mesh.uv = Misc.MapAll(instance._uvs, x => new Vector2((float)x.X, (float)x.Y)).ToArray();
			filter.mesh.triangles = instance.Triangles.ToArray();
			
			instance.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName.ToLower())
				{
					case "vertices":
						filter.mesh.vertices = Misc.MapAll(instance._vertices, Misc.MapVector3D).ToArray();
						break;
					case "uvs":
						filter.mesh.uv = Misc.MapAll(instance._uvs, x => new Vector2((float)x.X, (float)x.Y)).ToArray();
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
	}
}