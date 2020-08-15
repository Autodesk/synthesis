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

namespace Engine.ModuleLoader.Adapters
{
	public class SelectableAdapter : MonoBehaviour, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private static List<Selectable> selectables = new List<Selectable>(); // TODO manage lifetime
		private new MeshColliderAdapter collider;
		private Material[] materials;
		public const float FlashSelectedTime = 0.1f; // sec
		// private bool isPointerOnThis = false;

		public void SetInstance(Selectable obj)
		{
			instance = obj;
			selectables.Add(instance);
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
				foreach (var selectable in selectables)
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

		private EventTrigger.Entry MakeEventTriggerEntry(EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> action)
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = type;
			entry.callback.AddListener(action);
			return entry;
		}

		public void OnEnable()
		{
			if (instance == null)
			{
				gameObject.SetActive(false);
				return;
			}

			if (gameObject.GetComponent<EventTrigger>() == null)
			{
				var eventTrigger = gameObject.AddComponent<EventTrigger>();
				eventTrigger.triggers.Add(MakeEventTriggerEntry(EventTriggerType.PointerClick, data =>
				{
					if (((PointerEventData) data).button == PointerEventData.InputButton.Left) // TODO use preference manager for this
						Select();
				}));
				//eventTrigger.triggers.Add(MakeEventTriggerEntry(EventTriggerType.PointerEnter, data => isPointerOnThis = true));
				//eventTrigger.triggers.Add(MakeEventTriggerEntry(EventTriggerType.PointerExit,  data => isPointerOnThis = false));
			}
			if ((collider = gameObject.GetComponent<MeshColliderAdapter>()) == null)
				throw new Exception("Entity must have a mesh collider component");

			materials = GetComponent<MeshRenderer>().materials;
		}

		public void Start()
		{
			gameObject.transform.position = gameObject.transform.position + new Vector3(0, float.Epsilon, 0); // Enable Unity collider by moving transform slightly
		}

		public void Update()
		{
			/*
			if (collider.sharedMesh == null)
			{
				collider.sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
				materials = gameObject.GetComponent<MeshRenderer>().materials;
				if (collider.sharedMesh == null)
				{
					throw new System.Exception("Selectable entity does not have a mesh");
				}
			}*/

			/*
			if (Input.GetMouseButtonDown(0) && isPointerOnThis)
			{
				Select();
			}
			*/
			if (Input.GetMouseButtonDown(1)) // TODO use preference manager for this
			{
				Deselect();
			}
		}
		
		
	}

	
}