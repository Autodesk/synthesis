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

            var newTop = target.worldBound.y + diff.Y;
            var newLeft = target.worldBound.x + diff.X;

            if (newTop > -e.localMousePosition.y && (newTop + e.localMousePosition.y) < UnityEngine.Screen.height)
                target.style.top = (StyleLength)(target.layout.y + diff.Y);

            if (newLeft > -e.localMousePosition.x && (newLeft + e.localMousePosition.x) < UnityEngine.Screen.width)
                target.style.left = (StyleLength)newLeft;

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
