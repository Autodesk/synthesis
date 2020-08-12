using Engine.Util;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine;

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
					meshCollider.sharedMesh.vertices = Utilities.MapAllToArray(sprite._sprite.vertices, (v2d) => new Vector3(v2d.x, v2d.y, 0));
					meshCollider.sharedMesh.triangles = Utilities.MapAllToArray(sprite._sprite.triangles, (i) => (int)i);
					instance.Bounds._bounds = meshCollider.bounds;
					instance.ProcessedChanges();
				}
				if (Input.GetMouseButton(0)) // TODO use preference manager?
				{
					Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

					if (sprite.AlwaysOnTop)
					{
						// TODO block hits to other objects "below" this one
						var hits = Physics.RaycastAll(ray, Mathf.Infinity);
						foreach (var hit in hits)
						{
							if (hit.transform == transform)
							{
								instance.OnMouseDown();
							}
						}
					}
					else
					{
						Physics.Raycast(ray, out RaycastHit hit);

						if (hit.transform == transform)
						{
							instance.OnMouseDown();
						}
					}
                }
                else
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