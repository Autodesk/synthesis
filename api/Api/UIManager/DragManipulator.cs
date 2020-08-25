using MathNet.Spatial.Euclidean;
using SynthesisAPI.Utilities;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    /// <summary>
    /// A manipulator that makes a visual element draggable
    /// 
    /// Taken from a Unity demo at:
    /// https://github.com/Unity-Technologies/UIElementsUniteLATurretDemo/blob/master/Assets/Demo/Editor/TextureDragger.cs
    /// </summary>
    internal class DragManipulator : MouseManipulator
    {
        private Vector2D startPosition;
        protected bool isActive;

        public DragManipulator()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            isActive = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (isActive)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                startPosition = e.localMousePosition.Map();

                isActive = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!isActive || !target.HasMouseCapture())
                return;

            var diff = e.localMousePosition.Map() - startPosition;

            target.style.top = (StyleLength)Math.Clamp(target.layout.y + diff.Y, 0, target.parent.layout.height - target.layout.height);
            target.style.left = (StyleLength)Math.Clamp(target.layout.x + diff.X, 0, target.parent.layout.width - target.layout.width);

            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (!isActive || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            isActive = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }
}
