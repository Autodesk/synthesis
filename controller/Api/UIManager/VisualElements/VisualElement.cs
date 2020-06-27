using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class VisualElement
    {
        private UnityVisualElement _visualElement;

        public static implicit operator UnityVisualElement(VisualElement element) => element._visualElement;
        public static implicit operator VisualElement(UnityVisualElement element) => new VisualElement(element);

        public VisualElement()
        {
            _visualElement = ApiProvider.InstantiateFocusable<UnityVisualElement>();
        }

        internal VisualElement(UnityVisualElement visualElement)
        {
            _visualElement = visualElement;
        }

        public string Name {
            get => _visualElement.name;
            set => _visualElement.name = value;
        }
    }
}
