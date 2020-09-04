using System;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
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

        private EventBus.EventBus.EventCallback _changeCallback;
        private EventBus.EventBus.EventCallback _focusCallback;
        private bool _isChangeCallbackRegistered = false;
        private bool _isFocusCallbackRegistered = false;

        private static int Id = 0;
        private int id;

        public string ChangeEventTag => $"text-field-change/{Element.name}-{id}";
        public string FocusLeaveEventTag => $"text-field-focus-leave/{Element.name}-{id}";

        private protected _UnityTextField Element
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

        public bool IsDelayed
        {
            get => Element.isDelayed;
            set => Element.isDelayed = value;
        }

        public void SetValueWithoutNotify(string value)
        {
            Element.SetValueWithoutNotify(value);
        }

        public void SubscribeOnChange(Action<IEvent> action)
        {
            _changeCallback = e => action(e);
            EventBus.EventBus.NewTagListener(ChangeEventTag, _changeCallback);

            if (!_isChangeCallbackRegistered)
            {
                Element.RegisterCallback<UnityEngine.UIElements.ChangeEvent<string>>(e =>
                {
                    EventBus.EventBus.Push(ChangeEventTag, new TextFieldChangeEvent(Name, e.previousValue, e.newValue));
                });
                _isChangeCallbackRegistered = true;
            }
        }

        public void SubscribeOnFocusLeave(Action<IEvent> action)
        {
            _focusCallback = e => action(e);
            EventBus.EventBus.NewTagListener(FocusLeaveEventTag, _focusCallback);

            if (!_isFocusCallbackRegistered)
            {
                Element.RegisterCallback<UnityEngine.UIElements.FocusOutEvent>(_ =>
                {
                    EventBus.EventBus.Push(FocusLeaveEventTag, new TextFieldFocusLeaveEvent(Name));
                });
                _isFocusCallbackRegistered = true;
            }
        }

        public class TextFieldChangeEvent : IEvent
        {
            public readonly string Name;
            public readonly string PreviousValue;
            public readonly string NewValue;

            public TextFieldChangeEvent(string name, string previousValue, string newValue)
            {
                Name = name;
                PreviousValue = previousValue;
                NewValue = newValue;
            }
        }

        public class TextFieldFocusLeaveEvent : IEvent
        {
            public readonly string Name;

            public TextFieldFocusLeaveEvent(string name)
            {
                Name = name;
            }
        }

        public TextField()
        {
            Element = ApiProvider.CreateUnityType<_UnityTextField>()!;
            if (Element == null)
                throw new Exception();
            id = Id;
            Id++;
        }

        internal TextField(_UnityTextField element)
        {
            Element = element;
            id = Id;
            Id++;
        }
    }
}