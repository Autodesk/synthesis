using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityLabel = UnityEngine.UIElements.Label;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class SynLabel
    {
        private UnityLabel _element;

        public static implicit operator UnityLabel(SynLabel element) => element._element;
        public static implicit operator SynLabel(UnityLabel element) => new SynLabel(element);

        public string Text {
            get => _element.text;
            set => _element.text = value;
        }

        public SynLabel()
        {
            _element = ApiProvider.InstantiateFocusable<UnityLabel>();
        }

        public SynLabel(UnityLabel element)
        {
            _element = element;
        }
    }
}
