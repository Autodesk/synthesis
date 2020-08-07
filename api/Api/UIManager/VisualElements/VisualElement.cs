using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class VisualElement
    {
        protected UnityVisualElement _visualElement;

        public static explicit operator UnityVisualElement(VisualElement element) => element._visualElement;
        public static explicit operator VisualElement(UnityVisualElement element) => new VisualElement(element);

        public VisualElement()
        {
            _visualElement = ApiProvider.CreateUnityType<UnityVisualElement>()!;
            if (_visualElement == null)
                throw new Exception("Failed to instantiate VisualElement");
        }

        public VisualElement(UnityVisualElement visualElement)
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

        internal UnityVisualElement UnityVisualElement {
            get => _visualElement;
        }

        public virtual IEnumerable<Object> PostUxmlLoad()
        {
            foreach (var child in _visualElement.Children())
            {
                child.GetVisualElement().PostUxmlLoad();
            }
            return null;
        }

        public IEnumerable<VisualElement> GetChildren()
        {
            var children = new List<VisualElement>();
            foreach (var child in _visualElement.Children())
                children.Add(child.GetVisualElement());
            return children;
        }

        public VisualElement Get(string name = null, string className = null) => _visualElement.Q(name, className).GetVisualElement();
        public void Add(VisualElement element) => _visualElement.Add(element._visualElement);

        public void SetStyleProperty(string name, string value)
        {
            _visualElement = UIParser.ParseEntry($"{name}:{value}", _visualElement);
        }
        
        #region Dynamic Accessors

        /// <summary>
        /// Allows you to access <see cref="https://docs.unity3d.com/ScriptReference/UIElements.VisualElement.html">VisualElement</see>
        /// and still compile without the UnityEngine assembly. This is going to be protected until we need it
        /// </summary>
        protected virtual dynamic DynamicVisualElement
        {
            get => _visualElement;
            set => _visualElement = value is VisualElement ? value : _visualElement;
        }

        #endregion
    }
}
