using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
using _UnityButton = UnityEngine.UIElements.Button;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class Button : VisualElement
    {
        private EventBus.EventBus.EventCallback _callback;

        internal _UnityButton Element
        {
            get => (_visualElement as _UnityButton)!;
            set => _visualElement = value;
        }

        private static int Id = 0;
        private int id;

        public string EventTag
        {
            get => $"button/{Element.name}-{id}";
        }

        public string Text {
            get => Element.text;
            set => Element.text = value;
        }

        public Button()
        {
            _visualElement = ApiProvider.CreateUnityType<_UnityButton>()!;
            if (_visualElement == null)
                throw new Exception();
            id = Id;
            Id++;
        }

        internal Button(_UnityButton element)
        {
            Element = element;
            id = Id;
            Id++;
        }

        // Unsure if this is needed
        ~Button()
        {
            /*if (EventBus.EventBus.HasTagSubscriber(EventTag))
                EventBus.EventBus.RemoveTagListener(EventTag, _callback);*/
        }

        public override IEnumerable<object> PostUxmlLoad()
        {
            Element.clickable.clicked += () => EventBus.EventBus.Push(EventTag, new ButtonClickableEvent(Name));
            base.PostUxmlLoad();
            return null!;
        }

        public void Subscribe(Action<IEvent> action)
        {
            _callback = e => action(e);
            EventBus.EventBus.NewTagListener(EventTag, _callback);
            PostUxmlLoad();
        }
    }
}
