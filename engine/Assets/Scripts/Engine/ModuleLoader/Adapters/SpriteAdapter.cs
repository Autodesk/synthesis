using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine;
using Sprite = SynthesisAPI.EnvironmentManager.Components.Sprite;

namespace Engine.ModuleLoader.Adapters
{
	public sealed class SpriteAdapter : MonoBehaviour, IApiAdapter<Sprite>
	{
		private Sprite instance = null;
		private new SpriteRenderer renderer = null;
		private Material defaultMaterial;

		public void Awake()
		{
			if ((renderer = gameObject.GetComponent<SpriteRenderer>()) == null)
				renderer = gameObject.AddComponent<SpriteRenderer>();

			if (instance == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void Update()
		{
			if (instance.Changed)
			{
				renderer.sprite = instance._sprite;
				defaultMaterial = renderer.material;
				if (instance._alwaysOnTop)
					renderer.material = new Material(Shader.Find("Custom/AlwaysOnTop"));
				else
					renderer.material = defaultMaterial;

				renderer.flipX = instance._flipX;
				renderer.flipY = instance._flipY;
				renderer.color = new Color(instance._color.R, instance._color.G, instance._color.B, instance._color.A);
				var collider = instance.Entity?.GetComponent<MeshCollider2D>();

				if (collider != null)
				{
					collider.Changed = true;
				}
				instance.ProcessedChanges();
			}
		}

		public void SetInstance(Sprite sprite)
		{
			instance = sprite;
			gameObject.SetActive(true);
		}

		public static Sprite NewInstance()
		{
			return new Sprite();
		}
	}
}