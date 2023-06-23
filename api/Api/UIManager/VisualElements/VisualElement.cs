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
                throw new SynthesisException("Failed to instantiate VisualElement");
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

        public bool Enabled
        {
            get => _visualElement.enabledSelf;
            set => _visualElement.SetEnabled(value);
        }

        internal IStyle style {
            get => _visualElement.style;
        }

        public void OnMouseEnter(Action onEnter)
        {
            _visualElement.RegisterCallback<MouseEnterEvent>(_ => onEnter());
        }

        public void OnMouseLeave(Action onLeave)
        {
            _visualElement.RegisterCallback<MouseLeaveEvent>(_ => onLeave());
        }

        public void OnMouseDown(Action onMouseDown)
        {
            _visualElement.RegisterCallback<MouseDownEvent>(_ => onMouseDown());
        }

        internal _UnityVisualElement UnityVisualElement {
            get => _visualElement;
        }

        public virtual IEnumerable<Object>? PostUxmlLoad()
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

        public VisualElement? Get(string? name = null, string? className = null)
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

        private Manipulator? tooltipManipulator = null;
        
        private string tooltip = "";

        public string Tooltip
        {
            get => tooltip;
            set
            {
                if (value != tooltip)
                {
                    tooltip = value;
                    if (tooltip != "")
                    {
                        if (tooltipManipulator == null)
                        {
                            tooltipManipulator = new TooltipManipulator(tooltip);
                            _visualElement.AddManipulator(tooltipManipulator);
                        }
                        else
                        {
                            ((TooltipManipulator)tooltipManipulator).Text = tooltip;
                        }
                    }
                    else if (tooltipManipulator != null)
                    {
                        _visualElement.RemoveManipulator(tooltipManipulator);
                        tooltipManipulator = null;
                    }

                }
            }
        }
        
        private bool isDraggable = false;
        private Manipulator? dragManipulator = null;
        public bool IsDraggable
        {
            get => isDraggable;
            set
            {
                if (value != isDraggable)
                {
                    isDraggable = value;
                    if (isDraggable)
                    {
                        dragManipulator = new DragManipulator();
                        _visualElement.AddManipulator(dragManipulator);
                    }
                    else if (dragManipulator != null)
                    {
                        _visualElement.RemoveManipulator(dragManipulator);
                        dragManipulator = null;
                    }
                }
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
