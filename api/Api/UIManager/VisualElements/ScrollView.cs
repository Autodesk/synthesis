using System;
using System.Collections.Generic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Runtime;
using _UnityScrollView = UnityEngine.UIElements.ScrollView;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class ScrollView : VisualElement
    {
        private _UnityScrollView Element
        {
            get => (_visualElement as _UnityScrollView)!;
            set => _visualElement = value;
        }

        public ScrollView(_UnityScrollView visualElement)
        {
            Element = visualElement;
        }
        public ScrollView(VisualElement visualElement)
        {
            Element = (_UnityScrollView)visualElement.UnityVisualElement;
        }
        public ScrollView()
        {
            Element = ApiProvider.CreateUnityType<_UnityScrollView>()!;
            if (Element == null)
                throw new Exception();
        }
        public void ScrollTo(VisualElement visualElement) => Element.ScrollTo(visualElement.UnityVisualElement);

        protected override dynamic DynamicVisualElement
        {
            get => Element;
            set => Element = value is _UnityScrollView ? value : Element;
        }
    }
}