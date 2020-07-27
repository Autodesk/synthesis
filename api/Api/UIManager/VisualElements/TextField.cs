using System;
using SynthesisAPI.Runtime;
using UnityEngine.UIElements;
using UnityTextField = UnityEngine.UIElements.TextField;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class TextField: VisualElement
    {
        public UnityTextField Element
        {
            get => (_visualElement as UnityTextField)!;
            set => _visualElement = value;
        }

        public static explicit operator UnityTextField(TextField element) => (element._visualElement as UnityTextField)!;
        public static explicit operator TextField(UnityTextField element) => new TextField(element);

        public string Label
        {
            get => Element.label;
            set => Element.label = value;
        }
        
        public string Value
        {
            get => Element.value;
            set => Element.value = value;
        }

        public TextField()
        {
            Element = ApiProvider.CreateUnityType<UnityTextField>()!;
            if (Element == null)
                throw new Exception();
        }

        public TextField(UnityTextField element)
        {
            Element = element;
        }
        
        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is UnityTextField ? value : Element;
        }
    }
}