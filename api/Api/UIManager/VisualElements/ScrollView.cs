using System;
using SynthesisAPI.Runtime;
using _UnityScrollView = UnityEngine.UIElements.ScrollView;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class ScrollView : VisualElement
    {
        private protected _UnityScrollView Element
        {
            get => (_visualElement as _UnityScrollView)!;
            set => _visualElement = value;
        }

        internal ScrollView(_UnityScrollView visualElement)
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
    }
}