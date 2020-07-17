using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Entity = System.UInt32;

namespace Engine.ModuleLoader.Adapters
{
	public class SelectableAdapter : MonoBehaviour, IPointerClickHandler, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private static List<Selectable> selectables = new List<Selectable>(); // TODO manage lifetime
		private static EventSystem eventSystem = null;
		private new MeshCollider collider;
		private bool isPointerOnThis = false;

		public void SetInstance(Selectable obj)
		{
			instance = obj;
			selectables.Add(instance);
		}

		public static Selectable NewInstance()
		{
			return new Selectable();
		}

		private void Deselect()
		{
			foreach (var selectable in selectables)
			{
				selectable.SetSelected(false);
			}
			Selectable.ResetSelected();
		}

		private void Select()
		{
			Deselect();
			instance.SetSelected(true);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			foreach (var selectable in selectables)
			{
				selectable.SetSelected(false);
			}
			instance.SetSelected(true);
		}

		private EventTrigger.Entry MakeOnPointerClickEntry()
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerClick;
			entry.callback.AddListener(data => Select());
			return entry;
		}

		private EventTrigger.Entry MakeOnPointerEnterEntry()
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(data => isPointerOnThis = true);
			return entry;
		}

		private EventTrigger.Entry MakeOnPointerExitEntry()
		{
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener(data => isPointerOnThis = false);
			return entry;
		}

		public void Awake()
		{
			if (eventSystem == null)
			{
				eventSystem = Util.Utilities.FindGameObject("EventSystem").GetComponent<EventSystem>();
				if (eventSystem == null)
				{
					throw new System.Exception();
				}
			}
			if (gameObject.GetComponent<EventTrigger>() == null)
			{
				var eventTrigger = gameObject.AddComponent<EventTrigger>();
				eventTrigger.triggers.Add(MakeOnPointerClickEntry());
				eventTrigger.triggers.Add(MakeOnPointerEnterEntry());
				eventTrigger.triggers.Add(MakeOnPointerExitEntry());
			}
			if (gameObject.GetComponent<MeshCollider>() == null)
			{
				collider = gameObject.AddComponent<MeshCollider>();
				collider.convex = true; // Mesh collider wont have any holes
			}
		}

		public void Update()
		{
			if (collider.sharedMesh == null)
			{
				collider.sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
				if (collider.sharedMesh == null)
				{
					throw new System.Exception("Selectable entity does not have a mesh");
				}
			}
			if (Input.GetMouseButtonDown(1) && !isPointerOnThis) // Right click to deselect all -- TODO add to preference manager?
			{
				Deselect();
			}
		}
	}
}