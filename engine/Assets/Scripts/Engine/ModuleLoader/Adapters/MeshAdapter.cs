﻿using System.Collections.Generic;
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
			if ((renderer = gameObject.GetComponent<MeshRenderer>()) == null)
				renderer = gameObject.AddComponent<MeshRenderer>();
		}

		public void Update()
		{
			if (instance.Changed)
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