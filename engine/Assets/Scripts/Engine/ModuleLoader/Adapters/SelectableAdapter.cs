using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Engine.ModuleLoader.Adapters
{
    public class SelectableAdapter : UnityEngine.MonoBehaviour, IPointerClickHandler, IApiAdapter<Selectable>
	{
		private Selectable instance;
		private static List<Selectable> selectables = new List<Selectable>();
		private static EventSystem eventSystem = null;

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
			foreach(var selectable in selectables)
            {
				selectable.SetSelected(false);
            }
			instance.SetSelected(true);
		}

		public void Awake()
        {
			if(eventSystem == null)
            {
				eventSystem = Util.Util.FindGameObject("EventSystem").GetComponent<EventSystem>();
            }
        }
	}
}