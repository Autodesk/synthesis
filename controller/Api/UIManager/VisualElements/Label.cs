using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityLabel = UnityEngine.UIElements.Label;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class Label
    {
        private UnityLabel _element;

        public static implicit operator UnityLabel(Label element) => element._element;
        public static implicit operator Label(UnityLabel element) => new Label(element);

        public string Text {
            get => _element.text;
            set => _element.text = value;
        }

        public Label()
        {
            _element = ApiProvider.InstantiateFocusable<UnityLabel>();
        }

        public Label(UnityLabel element)
        {
            _element = element;
        }
    }
}
