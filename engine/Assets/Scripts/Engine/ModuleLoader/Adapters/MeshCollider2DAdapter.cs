using Engine.Util;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using static Engine.ModuleLoader.Api;

using MeshCollider = UnityEngine.MeshCollider;

namespace Engine.ModuleLoader.Adapters
{
    public sealed class MeshCollider2DAdapter : MonoBehaviour, IApiAdapter<MeshCollider2D>
	{
		private MeshCollider2D instance;
		private MeshCollider meshCollider = null;
		private SynthesisAPI.EnvironmentManager.Components.Sprite sprite = null;

		public void OnEnable()
		{
			if (instance == null)
			{
				gameObject.SetActive(false);
				return;
			}

			if (meshCollider == null)
			{
				meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = new UnityEngine.Mesh();
				meshCollider.convex = true;
			}
			InputManager.AssignDigitalInput($"_internal MeshCollider2DAdapter select", new Digital($"mouse 0 non-ui"), e => ProcessInput((DigitalEvent)e)); // TODO use preference manager for this
		}

		public void OnDestroy()
		{
			InputManager.UnassignDigitalInput($"_internal MeshCollider2DAdapter select");
		}

		public void Update()
		{
			sprite = instance.Entity?.GetComponent<SynthesisAPI.EnvironmentManager.Components.Sprite>();
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
			}
		}

		public void ProcessInput(DigitalEvent mouseDownEvent)
		{
			if (sprite != null)
			{
				if (mouseDownEvent.State == DigitalState.Down)
				{
					Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

					bool isAlwaysOnTop = instance.Entity?.GetComponent<AlwaysOnTop>() != null;
					bool hitIntercepted = false;
					bool hitMe = false;
					var hits = Physics.RaycastAll(ray, Mathf.Infinity);
					foreach (var hit in hits)
					{
						if (hit.collider.transform == transform)
						{
							hitMe = true;
						}
						else if (ApiProviderData.GameObjects.TryGetValue(hit.collider.transform.gameObject, out Entity otherE))
						{
							if (otherE.GetComponent<AlwaysOnTop>() != null)
							{
								hitIntercepted = true;
							}
							else
							{
								if (!hitMe)
								{

									hitIntercepted = true;
								}
							}
						}
					}
					if (hitMe && (!hitIntercepted || isAlwaysOnTop))
					{
						instance.OnMouseDown();
					}
				}
				else if (mouseDownEvent.State == DigitalState.Up)
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