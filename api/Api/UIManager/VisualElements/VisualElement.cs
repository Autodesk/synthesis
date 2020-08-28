using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using _UnityVisualElement = UnityEngine.UIElements.VisualElement;

namespace SynthesisAPI.UIManager.VisualElements
{
    public class VisualElement
    {
        private protected _UnityVisualElement _visualElement;

        public VisualElement()
        {
            _visualElement = ApiProvider.CreateUnityType<_UnityVisualElement>()!;
            if (_visualElement == null)
                throw new Exception("Failed to instantiate VisualElement");
        }

        internal VisualElement(_UnityVisualElement visualElement)
        {
            _visualElement = visualElement;
        }

        public string Name {
            get => _visualElement.name;
            set => _visualElement.name = value;
        }

        public bool Focusable
        {
            get => _visualElement.focusable;
            set => _visualElement.focusable = value;
        }

        /// <summary>
        /// TODO: tooltips do not seem to be supported by Unity yet
        /// </summary>
        public string Tooltip
        {
            get => _visualElement.tooltip;
            set => _visualElement.tooltip = value;
        }

        public bool Enabled
        {
            get => _visualElement.enabledSelf;
            set => _visualElement.SetEnabled(value);
        }

        public IStyle style {
            get => _visualElement.style;
        }

        internal _UnityVisualElement UnityVisualElement {
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
        public IEnumerable<VisualElement> GetAllChildrenWhere(Func<VisualElement, bool> predicate)
        {
            var children = new List<VisualElement>();
            foreach (var child in _visualElement.Children())
            {
                var synChild = child.GetVisualElement();
                if (predicate(synChild))
                {
                    children.Add(synChild);
                }
                children.AddRange(synChild.GetAllChildrenWhere(predicate));
            }
            return children;
        }

        public IEnumerable<VisualElement> GetAllChildren()
        {
            var children = new List<VisualElement>();
            foreach (var child in _visualElement.Children())
            {
                var synChild = child.GetVisualElement();
                children.Add(synChild);
                children.AddRange(synChild.GetAllChildren());
            }
            return children;
        }

        public IEnumerable<VisualElement> GetChildren()
        {
            var children = new List<VisualElement>();
            foreach (var child in _visualElement.Children())
                children.Add(child.GetVisualElement());
            return children;
        }

        public VisualElement Get(string name = null, string className = null)
        {
            if (_visualElement == null)
                return null;
            var found = _visualElement.Q(name, className);
            if (found == null)
                return null;
            try
            {
                return found.GetVisualElement();
            }
            catch (Exception)
            {
                return new VisualElement(found);
            }
        }

        public void OnLoseFocus(Action onLose) => _visualElement.RegisterCallback<FocusOutEvent>(_ => onLose());
        public void Focus() => _visualElement.Focus();

        public void Add(VisualElement element) => _visualElement.Add(element._visualElement);
        public void Remove(VisualElement element) => _visualElement.Remove(element._visualElement);
        public void RemoveAt(int index) => _visualElement.RemoveAt(index);
        public void Insert(int index, VisualElement element) => _visualElement.Insert(index, element._visualElement);
        public void AddToClassList(string className) => _visualElement.AddToClassList(className);
        public void RemoveFromClassList(string className) => _visualElement.RemoveFromClassList(className);
        public void RemoveFromHierarchy() => _visualElement.RemoveFromHierarchy();
        public IEnumerable<string> GetClasses() => _visualElement.GetClasses();
        public Vector2D Position { get => _visualElement.worldBound.position.Map(); }
        public Vector2D Size { get => _visualElement.worldBound.size.Map(); }
        public bool ContainsLocalPoint(Vector2D localPoint) => _visualElement.ContainsPoint(localPoint.Map());
        public bool ContainsPoint(Vector2D worldPoint) => _visualElement.ContainsPoint((worldPoint - Position).Map());
        public bool ClassesContains(string className) => _visualElement.ClassListContains(className);

        public void SetStyleProperty(string name, string value)
        {
            _visualElement = UIParser.ParseEntry($"{name}:{value}", _visualElement);
        }
    }
}
