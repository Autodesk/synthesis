using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Engine.ModuleLoader.Api;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.InputManager.InputEvents;
using System.Linq;
using SynthesisAPI.EventBus;

namespace Engine.ModuleLoader.Adapters
{
	public class SelectableAdapter : MonoBehaviour, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private static int selectableAdapterCount = 0;

		private static int selectableAdapterIndex = 0; // Used for InputManager control name
		private int myIndex = 0; // Used for InputManager control name // TODO make this unnecessary

		private List<Material> materials = new List<Material>();
		public const float FlashSelectedTime = 0.1f; // sec
		private long lastClickTime = 0; // ms

		private bool isDestroyed = false;

		public void SetInstance(Selectable obj)
		{
			instance = obj;
			gameObject.SetActive(true);
			myIndex = selectableAdapterIndex;
			selectableAdapterIndex++;
			selectableAdapterCount++;
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
						selectable.SetSelected(Selectable.SelectionType.Unselected);
						selectable.OnDeselect();
					}
				}
				Selectable.ResetSelected();
			}
		}

		private void Select()
		{
			// Debug.Log("Select()");
			var currentClickTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
			var lastSelected = Selectable.Selected;
			if (!instance.IsSelected)
			{
				Deselect();
				instance.SetSelected((currentClickTime - lastClickTime) <= 400 ? Selectable.SelectionType.ExtendedSelection : Selectable.SelectionType.ExtendedSelectionPending);
				instance.OnSelect();
				EventBus.Push(new Selectable.SelectionChangeEvent(Selectable.Selected, lastSelected));

				StartCoroutine(FlashYellow());
			}
			else
			{
				if ((currentClickTime - lastClickTime) <= 400)
					instance.SetSelected(Selectable.SelectionType.ExtendedSelection);
				else
					instance.SetSelected(Selectable.SelectionType.Selected);
			}
			lastClickTime = currentClickTime;
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
					foreach (var material in renderer.materials)
					{
						if (!materials.Any(m => ReferenceEquals(m, material)))
						{
							materials.Add(material);
						}
					}
				}
			}
			else
			{
				foreach (var ms in GetComponentsInChildren<MeshRenderer>())
				{
					foreach (var material in ms.materials)
					{
						if (!materials.Any(m => ReferenceEquals(m, material)))
						{
							materials.Add(material);
						}
					}
				}
			}
			InputManager.AssignDigitalInput($"_internal SelectableAdapter select {myIndex}", new Digital($"mouse 0 non-ui"), e => {
				if (!isDestroyed)
					ProcessInput((DigitalEvent)e);
			}); // TODO use preference manager for this

			if (selectableAdapterCount == 0)
			{
				InputManager.AssignDigitalInput($"_internal SelectableAdapter deselect", new Digital($"mouse 1 non-ui"), e =>
				{
					if (Selectable.Selected != null)
					{
						var lastSelected = Selectable.Selected;
						Deselect();
						EventBus.Push(new Selectable.SelectionChangeEvent(Selectable.Selected, lastSelected));
					}
				});
			}
		}

		public void OnDestroy()
		{
			isDestroyed = true;
			InputManager.UnassignDigitalInput($"_internal SelectableAdapter select {myIndex}");
			if (selectableAdapterCount == 0)
			{
				InputManager.UnassignDigitalInput($"_internal SelectableAdapter deselect {myIndex}");
			}
			if (Selectable.Selected?.Entity != null && Selectable.Selected?.Entity == instance.Entity)
			{
				var lastSelected = Selectable.Selected;
				Deselect();
				EventBus.Push(new Selectable.SelectionChangeEvent(Selectable.Selected, lastSelected));
			}
			selectableAdapterCount--;
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