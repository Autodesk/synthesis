using System;
using SynthesisAPI.Runtime;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynTextField: SynVisualElement
    {
        public TextField Element
        {
            get => (_visualElement as TextField)!;
            set => _visualElement = value;
        }

        public static explicit operator TextField(SynTextField element) => (element._visualElement as TextField)!;
        public static explicit operator SynTextField(TextField element) => new SynTextField(element);

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

        public SynTextField()
        {
            Element = ApiProvider.InstantiateFocusable<TextField>()!;
            if (Element == null)
                throw new Exception();
        }

        public SynTextField(TextField element)
        {
            Element = element;
        }
        
        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is TextField ? value : Element;
        }
    }
}