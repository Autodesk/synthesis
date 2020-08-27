using SynthesisAPI.Runtime;
using System.Collections;
using UnityEngine.UIElements;

namespace SynthesisAPI.UIManager
{
    /// <summary>
    /// A manipulator that adds a tooltip to a visual element
    /// </summary>
    internal class TooltipManipulator : MouseManipulator
    {
        private const int HoverDuration = 1000; // ms

        private static UnityEngine.Vector2 mousePosition;

        private static bool isTooltipOpen;
        private static IEnumerator timerCoroutine = null;

        private static VisualElements.VisualElement tooltipContainer = null;
        private static VisualElements.Label tooltip = null;

        public string Text;

        public TooltipManipulator(string text = "")
        {
            if (tooltipContainer == null)
            {
                tooltipContainer = UIManager.RootElement.Get("tooltip-container");
                tooltipContainer.UnityVisualElement.RegisterCallback<MouseMoveEvent>(_ => CancelTooltips());
                tooltipContainer.UnityVisualElement.RegisterCallback<MouseLeaveEvent>(_ => CancelTooltips());
                tooltipContainer.UnityVisualElement.RegisterCallback<MouseDownEvent>(_ => CancelTooltips());
            }

            isTooltipOpen = false;
            Text = text;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseLeave(MouseLeaveEvent e)
        {
            if (!tooltipContainer.Enabled)
            {
                CancelTooltips();
            }
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            CancelTooltips();

            mousePosition = e.mousePosition;

            timerCoroutine = StartTimer();
            ApiProvider.StartCoroutine(timerCoroutine);

            e.StopPropagation();
        }

        private IEnumerator StartTimer()
        {
            yield return new UnityEngine.WaitForSeconds(HoverDuration / 1000f);
                
            OpenTooltip();
        }

        private void OpenTooltip()
        {
            if (!isTooltipOpen && Text != "")
            {
                isTooltipOpen = true;
                if (tooltip == null)
                {
                    tooltip = new VisualElements.Label
                    {
                        Name = "test-tooltip"
                    };

                    tooltip.AddToClassList("tooltip");
                    
                    tooltip.SetStyleProperty("height", "25px");
                }

                tooltip.SetStyleProperty("top", $"{mousePosition.y + 15}px");
                tooltip.SetStyleProperty("left", $"{mousePosition.x + 12}px");

                tooltip.Text = Text;

                double width = tooltip.Text.Length * 6.3;
                tooltip.SetStyleProperty("width", $"{(int)width}px");
                
                tooltipContainer.Add(tooltip);
                tooltipContainer.Enabled = true;
            }
        }

        private void CloseTooltip()
        {
            if (isTooltipOpen)
            {
                tooltip.RemoveFromHierarchy();
                tooltipContainer.Enabled = false;
                isTooltipOpen = false;
            }
        }

        private void CancelTooltips()
        {
            if (timerCoroutine != null)
            {
                ApiProvider.StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            if (isTooltipOpen)
            {
                CloseTooltip();
            }
        }
    }
}
