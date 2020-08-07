using System;
using SynthesisAPI.Runtime;
using _UnityTextField = UnityEngine.UIElements.TextField;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class TextField: VisualElement
    {
        /*
        /// <summary>
        /// This wrapper exposes the protected member text of the Unity TextField
        /// </summary>
        internal class UnityTextFieldWrapper: _UnityTextField
        {
            public new string text
            {
                get => base.text;
                set => base.text = value;
            }
        }
        */
        

        internal _UnityTextField Element
        {
            get => (_visualElement as _UnityTextField)!;
            set => _visualElement = value;
        }

        //public static explicit operator _UnityTextField(TextField element) => (element._visualElement as _UnityTextField)!;
        //public static explicit operator TextField(_UnityTextField element) => new TextField(element);

        public string Label
        {
            get => Element.label;
            set => Element.label = value;
        }

        /*
        public string Text
        {
            get => Element.text;
            set => Element.text = value;
        }
        */

        public string Value
        {
            get => Element.value;
            set => Element.value = value;
        }

        public bool IsReadOnly
        {
            get => Element.isReadOnly;
            set => Element.isReadOnly = value;
        }

        public bool IsMultiline
        {
            get => Element.multiline;
            set => Element.multiline = value;
        }

        public void SetValueWithoutNotify(string value)
        {
            Element.SetValueWithoutNotify(value);
        }

        public TextField()
        {
            Element = ApiProvider.CreateUnityType<_UnityTextField>()!;
            if (Element == null)
                throw new Exception();
        }

        public TextField(_UnityTextField element)
        {
            Element = element;
        }

        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is _UnityTextField ? value : Element;
        }
    }
}