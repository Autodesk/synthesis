using System;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
using UnityEngine;
using UnityEngine.EventSystems;
using MeshCollider = SynthesisAPI.EnvironmentManager.Components.MeshCollider;
using Mesh = SynthesisAPI.EnvironmentManager.Components.Mesh;
using static Engine.ModuleLoader.Api;

namespace Engine.ModuleLoader.Adapters
{
	public class SelectableAdapter : MonoBehaviour, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private new MeshColliderAdapter collider;
		private Material[] materials;
		public const float FlashSelectedTime = 0.1f; // sec

		public void SetInstance(Selectable obj)
		{
			instance = obj;
			gameObject.SetActive(true);
		}

		public static Selectable NewInstance()
		{
			return new Selectable();
		}

		private void Deselect()
		{
			if (instance.IsSelected)
			{
				instance.SetSelected(false);
				instance.OnDeselect();
			}
			if (Selectable.Selected != null)
			{
				foreach (var selectable in EnvironmentManager.GetComponentsWhere<Selectable>(c => true))
				{
					selectable.SetSelected(false);
				}
				Selectable.ResetSelected();
			}
		}

		private void Select()
		{
			if (!instance.IsSelected)
			{
				Deselect();
				instance.SetSelected(true);
				instance.OnSelect();

				StartCoroutine(FlashYellow());
			}
		}

		private IEnumerator FlashYellow() // TODO maybe make it highlight the mesh using some kind of shader
		{
			List<Color> backupColors = new List<Color>();
			foreach (var m in materials)
			{
				backupColors.Add(m.color);
				m.color = Color.yellow;
			}

			yield return new WaitForSeconds(FlashSelectedTime);

			for (var i = 0; i < materials.Length; i++)
			{
				materials[i].color = backupColors[i];
			}
		}

		public void OnEnable()
		{
			if (instance == null)
			{
				gameObject.SetActive(false);
				return;
			}

			if ((collider = gameObject.GetComponent<MeshColliderAdapter>()) == null)
				throw new Exception("Entity must have a mesh collider component");

			materials = GetComponent<MeshRenderer>().materials;
		}

		public void Update()
		{
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
					Select();
				}
			}
			if (Input.GetMouseButtonDown(1)) // TODO use preference manager for this
			{
				Deselect();
			}
		}
	}
}