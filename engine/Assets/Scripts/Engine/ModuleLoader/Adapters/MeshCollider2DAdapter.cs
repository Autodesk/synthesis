using Engine.Util;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System;
using UnityEngine;
using static Engine.ModuleLoader.Api;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class MeshCollider2DAdapter : MonoBehaviour, IApiAdapter<MeshCollider2D>
	{
		private MeshCollider2D instance;
		private MeshCollider meshCollider = null;

		public void Awake()
		{
			if (meshCollider == null)
			{
				meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = new UnityEngine.Mesh();
				meshCollider.convex = true;
			}
			if (instance == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void Update()
		{
			var sprite = instance.Entity?.GetComponent<SynthesisAPI.EnvironmentManager.Components.Sprite>();
			if (sprite != null)
			{
				if (instance.Changed)
				{
					meshCollider.sharedMesh.triangles = new int[0];
					meshCollider.sharedMesh.vertices = Misc.MapAllToArray(sprite._sprite.vertices, (v2d) => new Vector3(v2d.x, v2d.y, 0));
					meshCollider.sharedMesh.triangles = Misc.MapAllToArray(sprite._sprite.triangles, (i) => (int)i);
					instance.Bounds._bounds = meshCollider.bounds;
					instance.ProcessedChanges();
				}
				if (Input.GetMouseButtonDown(0)) // TODO use preference manager?
				{
					Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

					// TODO block hits to other objects "below" this one
					bool isAlwaysOnTop = instance.Entity?.GetComponent<AlwaysOnTop>() != null;
					bool hitAlwaysOnTop = false;
					bool hitMe = false;
					var hits = Physics.RaycastAll(ray, Mathf.Infinity);
					foreach (var hit in hits)
					{
						if (ApiProviderData.GameObjects.TryGetValue(hit.transform.gameObject, out Entity otherE))
						{
							if (otherE.GetComponent<AlwaysOnTop>() != null)
							{
								hitAlwaysOnTop = true;
							}
						}
						if (hit.transform == transform)
						{
							hitMe = true;
						}
					}
					if (hitMe && (isAlwaysOnTop || !hitAlwaysOnTop))
					{
						instance.OnMouseDown();
					}
				}
				else if(Input.GetMouseButtonUp(0))
				{
					instance.OnMouseUp();
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