using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityLabel = UnityEngine.UIElements.Label;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class Label : VisualElement
    {
        private UnityLabel Element
        {
            get => (_visualElement as UnityLabel)!;
            set => _visualElement = value;
        }

        public static explicit operator UnityLabel(Label element) => element.Element;
        public static implicit operator Label(UnityLabel element) => new Label(element);

        public string Text {
            get => Element.text;
            set => Element.text = value;
        }

        public (float r, float g, float b, float a) Color {
            get => (style.color.value.r, style.color.value.g, style.color.value.b, style.color.value.a);
            set => style.color = new UnityEngine.UIElements.StyleColor(new UnityEngine.Color(value.r, value.g, value.b, value.a));
        }

        public Label()
        {
            Element = ApiProvider.CreateUnityType<UnityLabel>()!;
            if (Element == null)
                throw new Exception();
        }

        public Label(UnityLabel element)
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
