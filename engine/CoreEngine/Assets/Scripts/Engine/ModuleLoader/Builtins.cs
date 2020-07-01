using System;
using System.ComponentModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using Engine.ModuleLoader;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Mesh = SynthesisAPI.Modules.Components.Mesh;
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
				filter.mesh.vertices = instance.Vertices.ToArray();
				filter.mesh.uv = instance.UVs.ToArray();
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
	}
}