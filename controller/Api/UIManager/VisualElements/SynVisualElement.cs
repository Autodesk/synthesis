using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

[assembly: InternalsVisibleTo("Synthesis.Core")]
namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynVisualElement
    {
        private VisualElement _visualElement;

        public static implicit operator VisualElement(SynVisualElement element) => element._visualElement;
        public static implicit operator SynVisualElement(VisualElement element) => new SynVisualElement(element);

        public SynVisualElement()
        {
            _visualElement = ApiProvider.InstantiateFocusable<VisualElement>();
        }

        internal SynVisualElement(VisualElement visualElement)
        {
            _visualElement = visualElement;
        }

        public string Name {
            get => _visualElement.name;
            set => _visualElement.name = value;
        }

        public IStyle style {
            get => _visualElement.style;
        }

        internal VisualElement VisualElement {
            get => _visualElement;
        }

        public T Get<T>(string name = null, string className = null) where T : VisualElement => _visualElement.Q<T>(name, className);
    }
}
