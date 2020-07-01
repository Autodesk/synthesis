using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityButton = UnityEngine.UIElements.Button;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynButton : SynVisualElement
    {
        private UnityButton _element;

        public static implicit operator UnityButton(SynButton element) => element._element;
        public static implicit operator SynButton(UnityButton element) => new SynButton(element);

        public string Text {
            get => _element.text;
            set => _element.text = value;
        }

        public SynButton()
        {
            _element = ApiProvider.InstantiateFocusable<UnityButton>();
        }

        public SynButton(UnityButton element)
        {
            _element = element;
        }

        public void Subscribe(Action callback)
        {
            _element.clickable.clicked += callback;
        }
    }
}
