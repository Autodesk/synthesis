using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityButton = UnityEngine.UIElements.Button;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class Button : VisualElement
    {
        private UnityButton _element;

        public static implicit operator UnityButton(Button element) => element._element;
        public static implicit operator Button(UnityButton element) => new Button(element);

        public string Text {
            get => _element.text;
            set => _element.text = value;
        }

        public Button()
        {
            _element = ApiProvider.InstantiateFocusable<UnityButton>();
        }

        public Button(UnityButton element)
        {
            _element = element;
        }

        public void Subscribe(Action callback)
        {
            _element.clickable.clicked += callback;
        }
    }
}
