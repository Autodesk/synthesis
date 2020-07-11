using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.EventBus;
using UnityEngine;
using UnityButton = UnityEngine.UIElements.Button;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynButton : SynVisualElement
    {
        private EventBus.EventBus.EventCallback _callback;
        
        public UnityButton Element
        {
            get => (_visualElement as UnityButton)!;
            set => _visualElement = value;
        }

        public string EventTag
        {
            get => $"button/{Element.name}";
        }
        
        public static explicit operator UnityButton(SynButton element) => element.Element;
        public static explicit operator SynButton(UnityButton element) => new SynButton(element);

        public string Text {
            get => Element.text;
            set => Element.text = value;
        }

        public SynButton()
        {
            _visualElement = ApiProvider.InstantiateFocusable<UnityButton>()!;
            if (_visualElement == null)
                throw new Exception();
        }

        public SynButton(UnityButton element)
        {
            Element = element;
        }

        // Unsure if this is needed
        ~SynButton()
        {
            /*if (EventBus.EventBus.HasTagSubscriber(EventTag))
                EventBus.EventBus.RemoveTagListener(EventTag, _callback);*/
        }

        public override IEnumerable<object> PostUxmlLoad()
        {
            Element.clickable.clicked += () => EventBus.EventBus.Push(EventTag, new ButtonClickableEvent());
            base.PostUxmlLoad();
            return null!;
        }

        public void Subscribe(Action<IEvent> action)
        {
            _callback = e => action(e);
            EventBus.EventBus.NewTagListener(EventTag, _callback);
            PostUxmlLoad();
        }
        
        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is UnityButton ? value : Element;
        }
    }
}
