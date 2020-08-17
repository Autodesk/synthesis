using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Engine.ModuleLoader.Api;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.InputManager.InputEvents;

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
			InputManager.AssignDigitalInput($"_internal selectable select", new Digital($"mouse 0 non-ui"), e => ProcessInput((DigitalEvent)e)); // TODO use preference manager for this
			InputManager.AssignDigitalInput($"_internal selectable deselect", new Digital($"mouse 1"), e =>
			{
				if (Selectable.Selected != null)
				{
					Deselect();
				}
			});
		}

		public void OnDestroy()
		{
			InputManager.UnassignDigitalInput($"_internal SelectableAdapter select");
			InputManager.UnassignDigitalInput($"_internal SelectableAdapter deselect");
		}

		public void ProcessInput(DigitalEvent mouseDownEvent)
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
		}
	}
}