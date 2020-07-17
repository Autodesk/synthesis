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

		public void SetInstance(Selectable obj)
		{
			instance = obj;
			selectables.Add(instance);
		}

		public static Selectable NewInstance()
		{
			return new Selectable();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			foreach (var selectable in selectables)
			{
				selectable.SetSelected(false);
			}
			instance.SetSelected(true);
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
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerClick;
				entry.callback.AddListener(data => OnPointerClick((PointerEventData)data));
				eventTrigger.triggers.Add(entry);
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
		}
	}
}