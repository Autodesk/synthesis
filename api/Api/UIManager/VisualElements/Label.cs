using SynthesisAPI.Runtime;
using System;
using _UnityLabel = UnityEngine.UIElements.Label;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class Label : VisualElement
    {
        private _UnityLabel Element
        {
            get => (_visualElement as _UnityLabel)!;
            set => _visualElement = value;
        }

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
            Element = ApiProvider.CreateUnityType<_UnityLabel>()!;
            if (Element == null)
                throw new Exception();
        }

        internal Label(_UnityLabel element)
        {
            Element = element;
        }
        
        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is _UnityLabel ? value : Element;
        }
    }
}
