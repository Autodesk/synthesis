using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityLabel = UnityEngine.UIElements.Label;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynLabel : SynVisualElement
    {
        private UnityLabel Element
        {
            get => (_visualElement as UnityLabel)!;
            set => _visualElement = value;
        }

        public static explicit operator UnityLabel(SynLabel element) => element.Element;
        public static implicit operator SynLabel(UnityLabel element) => new SynLabel(element);

        public string Text {
            get => Element.text;
            set => Element.text = value;
        }

        public SynLabel()
        {
            Element = ApiProvider.InstantiateFocusable<UnityLabel>()!;
            if (Element == null)
                throw new Exception();
        }

        public SynLabel(UnityLabel element)
        {
            Element = element;
        }
        
        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is UnityLabel ? value : Element;
        }
    }
}
