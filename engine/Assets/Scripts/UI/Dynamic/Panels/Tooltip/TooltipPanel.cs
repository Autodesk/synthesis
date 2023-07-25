using System;
using System.Threading;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Panels.Tooltip {
    public class TooltipPanel : PanelDynamic {
        private const float TOOLTIP_HEIGHT = 26;
        private const float HOZ_SPACING    = 15;

        private const float KEY_CHAR_WIDTH = 12;
        private const float KEY_PADDING    = 9f;

        private const float DESC_CHAR_WIDTH = 13;
        private const float DESC_PADDING    = 12;

        private static readonly Vector2 ICON_SIZE = new(20, 20);

        private static float CalcContentWidth((string key, string description)[] tooltips) {
            float maxWidth = 0;
            tooltips.ForEach(t => {
                float tooltipWidth = (t.key.Length * KEY_CHAR_WIDTH + KEY_PADDING) +
                                     (t.description.Length * DESC_CHAR_WIDTH + DESC_PADDING);
                maxWidth = Mathf.Max(tooltipWidth, maxWidth);
            });

            return maxWidth;
        }

        private static float CalcContentHeight((string key, string description)[] tooltips) {
            return (TOOLTIP_HEIGHT + 7.5f) * tooltips.Length - 49;
        }

        (string key, string description)[] _tooltips;

        // TODO: Remove when merged with the rest of the UI and use the vertical layout in dynamic components
        private static readonly Func<UIComponent, UIComponent> VERTICAL_LAYOUT = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public TooltipPanel((string key, string description)[] tooltips)
            : base(new Vector2(CalcContentWidth(tooltips), CalcContentHeight(tooltips))) {
            _tooltips = tooltips;
        }

        public override bool Create() {
            Title.SetText("");
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            iconObj.transform.SetParent(HeaderRt);

            var iconContent =
                new Content(null, iconObj, ICON_SIZE)
                    .StepIntoImage(i => i.SetSprite(SynthesisAssetCollection.GetSpriteByName("info-icon-white-solid")))
                    .SetAnchoredPosition<Image>(Vector3.zero);

            // Align top center
            var transform              = base.UnityObject.GetComponent<RectTransform>();
            transform.pivot            = new Vector2(0.5f, 1f);
            transform.anchorMax        = new Vector2(0.5f, 1f);
            transform.anchorMin        = new Vector2(0.5f, 1f);
            transform.anchoredPosition = new Vector2(0.0f, -38.0f);

            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.RootGameObject.SetActive(false);

            MainContent.RootRectTransform.Translate(new Vector2(0, 5));

            CreateTooltips();

            return true;
        }

        private void CreateTooltips() {
            Extensions.ForEach(_tooltips, kvp => {
                var (keyContent, descriptionContent) =
                    MainContent.CreateSubContent(new Vector2(MainContent.Size.x, TOOLTIP_HEIGHT))
                        .ApplyTemplate(VERTICAL_LAYOUT)
                        .SplitLeftRight((KEY_CHAR_WIDTH * kvp.key.Length) + KEY_PADDING, HOZ_SPACING);

                keyContent
                    .StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.BackgroundSecondary).SetCornerRadius(5))
                    .CreateLabel(TOOLTIP_HEIGHT)
                    .SetText(kvp.key)
                    .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                    .SetFontSize(20);

                descriptionContent.CreateLabel(TOOLTIP_HEIGHT).SetText(kvp.description).SetFontSize(20);
                ;
            });
        }

        public override void Update() {}

        public override void Delete() {}
    }

    public static class TooltipManager {
        private const float TOOLTIP_TIMEOUT_SEC = 7;

        private static TooltipPanel _currentTooltip;
        private static CancellationTokenSource _cts;

        /// <summary>Creates a new tooltip at the top center of the screen. Closes any active tooltip</summary>
        public static void CreateTooltip(params(string key, string description)[] tooltips) {
            if (_currentTooltip != null)
                DynamicUIManager.ClosePanel<TooltipPanel>();

            if (_cts != null) {
                _cts.Token.ThrowIfCancellationRequested();
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            TooltipTimeout(_cts.Token);

            DynamicUIManager.CreatePanel<TooltipPanel>(persistent: true, args: tooltips);
        }

        /// <summary>Closes the active tooltip if there is one</summary>
        public static void CloseTooltip() {
            DynamicUIManager.ClosePanel<TooltipPanel>();
        }

        /// <summary>Closes the current tooltip after a delay but not if canceled</summary>
        /// <param name="ct">A cancellation token to end this task if a new tooltip is created</param>
        private static async Task TooltipTimeout(CancellationToken ct) {
            float startTime = Time.time;
            while (true) {
                if (Time.time > startTime + TOOLTIP_TIMEOUT_SEC) {
                    CloseTooltip();
                    return;
                }

                try {
                    await Task.Delay(100, ct);
                } catch {
                    return;
                }
            }
        }
    }
}