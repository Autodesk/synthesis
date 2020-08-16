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
using SynthesisAPI.Utilities;

namespace Engine.ModuleLoader.Adapters
{
	public class SelectableAdapter : MonoBehaviour, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private List<Material> materials = new List<Material>();
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
			if (Selectable.Selected != null)
			{
				foreach (var selectable in EnvironmentManager.GetComponentsWhere<Selectable>(c => true))
				{
					if (selectable.IsSelected)
					{
						selectable.SetSelected(false);
						instance.OnDeselect();
					}
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
			List<Color> backupColors = new List<Color>(materials.Count);
			foreach (var m in materials)
			{
				backupColors.Add(m.color);
				m.color = Color.yellow;
			}

			yield return new WaitForSeconds(FlashSelectedTime);

			for (var i = 0; i < materials.Count; i++)
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

			if (gameObject.GetComponent<MeshColliderAdapter>() == null)
			{
				instance.UsingChildren = true;
			}

			if (!instance.UsingChildren)
			{
				var renderer = GetComponent<MeshRenderer>();
				if (renderer != null)
				{
					materials.AddRange(GetComponent<MeshRenderer>().materials);
				}
			}
			else
			{
				foreach (var m in GetComponentsInChildren<MeshRenderer>())
				{
					materials.AddRange(m.materials);
				}
			}
		}

		public void Update()
		{
			if (Input.GetMouseButtonDown(0)) // TODO use preference manager?
			{
				Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

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
							if (instance.UsingChildren)
							{
								Entity parent = otherE;
								while (parent != 0)
								{
									parent = parent.GetComponent<Parent>().ParentEntity;
									if (parent == instance.Entity)
									{
										hitMe = true;
										break;
									}
								}
							}
							if (!hitMe)
							{

								hitIntercepted = true;
							}
						}
					}
				}
				if (hitMe && (!hitIntercepted || isAlwaysOnTop))
				{
					Select();
				}
			}
			if (Selectable.Selected != null && Input.GetMouseButtonDown(1)) // TODO use preference manager for this
			{
				Deselect();
			}
		}
	}
}