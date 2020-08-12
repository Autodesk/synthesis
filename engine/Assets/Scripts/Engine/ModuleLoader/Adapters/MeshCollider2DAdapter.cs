using Engine.Util;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine.ModuleLoader.Adapters
{
    public sealed class MeshCollider2DAdapter : MonoBehaviour, IApiAdapter<MeshCollider2D>
	{
		private MeshCollider2D instance;
		private PolygonCollider2D polygonCollider = null;
		private MeshCollider meshCollider = null;

		private bool addMeshColliderNext = false;
		private UnityEngine.Mesh mesh;

		public void Awake()
		{
			if (gameObject.GetComponent<EventTrigger>() == null)
			{
				var eventTrigger = gameObject.AddComponent<EventTrigger>();
				eventTrigger.triggers.Add(Utilities.MakeEventTriggerEntry(EventTriggerType.PointerClick, data =>
				{
					if (((PointerEventData)data).button == PointerEventData.InputButton.Left) // TODO use preference manager for this
						instance.OnClick();
				}));
			}

			if (instance == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void Update()
		{
			if (addMeshColliderNext && gameObject.GetComponent<PolygonCollider2D>() == null && mesh != null)
			{
				if (meshCollider == null)
					meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.convex = true;
				meshCollider.sharedMesh = mesh;
				instance.Mesh.Vertices = (List<Vector3D>)Utilities.MapAll(mesh.vertices, MathUtil.MapVector3);
				instance.Mesh.UVs = (List<Vector2D >)Utilities.MapAll(mesh.uv, MathUtil.MapVector2);
				instance.Mesh.Triangles = new List<int>(mesh.triangles);
				addMeshColliderNext = false;
			}
			if (instance.Changed)
			{
				if (meshCollider != null)
				{
					Destroy(meshCollider);
				}
				else
				{
					polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
					mesh = polygonCollider.CreateMesh(true, true);
					Destroy(polygonCollider);
					addMeshColliderNext = true;
					instance.ProcessedChanges();
				}
			}
		}

		public void SetInstance(MeshCollider2D meshCollider2D)
		{
			instance = meshCollider2D;
			gameObject.SetActive(true);
		}

		public static MeshCollider2D NewInstance()
		{
			return new MeshCollider2D();
		}
	}
}