using System;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;
using Object = UnityEngine.Object;

using UImage = UnityEngine.UI.Image;

#nullable enable

namespace Synthesis.Util {
    public static class Toaster {

        private const int MAX_TOASTS = 20;
        private const float TOAST_TIMEOUT = 30f;
        private const int MAX_SHOWN_TOASTS = 5;
        public const float TOAST_SPACING = 15f;

        public static readonly Vector2 TOAST_CONTAINER_OFFSET = new Vector2 { x = -30f, y = 80f };
        public const float TOAST_CONTAINER_WIDTH = 500f;
        public const float TOAST_HEIGHT = 70f;
        public const float DROP_SHADOW_DISTANCE = 3f;
        public const float TOAST_CONTAINER_HEIGHT = TOAST_HEIGHT * (MAX_SHOWN_TOASTS + 1) + TOAST_SPACING * MAX_SHOWN_TOASTS;
        public static readonly Color DROP_SHADOW_MOD_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        private static readonly LinkedList<ToastHandler> TOASTS = new();

        private static ToasterComponent? _toasterComp;
        
        private static void EnsureComponent() {
            if (_toasterComp != null)
                return;
        
            _toasterComp = new GameObject("ToasterComp").AddComponent<ToasterComponent>();
            Object.DontDestroyOnLoad(_toasterComp.gameObject);
        }

        public static void MakeToast(string message, string? title = null, LogLevel level = LogLevel.Info,
            Action<ToastHandler>? closeCallback = null, Action<ToastHandler>? optionalCallback = null
            ) {
            
            EnsureComponent();
            
            var handler = new ToastHandler(message, title, level, closeCallback, optionalCallback);
            handler.LinkedNode = TOASTS.AddFirst(handler);
            while (TOASTS.Count > MAX_TOASTS) {
                var last = TOASTS.Last;
                TOASTS.RemoveLast();
                last.Value.Dispose();
            }

            UpdatePositions();
        }

        public static void RemoveToast(LinkedListNode<ToastHandler> node) {
            TOASTS.Remove(node);
            node.Value.Dispose();
            UpdatePositions();
        }

        private static void UpdatePositions() {
            float height = 0f;
            foreach (var toast in TOASTS) {
                toast?.SetHeight(height);
                if (toast != null)
                    height += TOAST_SPACING + TOAST_HEIGHT;
            }
        }

        private class ToasterComponent : MonoBehaviour {
            private void Update() {
                while (TOASTS.Count > 0 && Time.realtimeSinceStartup - TOASTS.Last.Value.CreationTime > TOAST_TIMEOUT) {
                    RemoveToast(TOASTS.Last);
                }
            }
        }
    }

    public class ToastHandler: IDisposable {

        private Content? _toastMainContent;
        private readonly string _tweenKey;
        public LinkedListNode<ToastHandler>? LinkedNode;
        public readonly float CreationTime;

        public ToastHandler(string message, string? title = null, LogLevel level = LogLevel.Debug,
            Action<ToastHandler>? closeCallback = null, Action<ToastHandler>? optionalCallback = null
            ) {

            CreationTime = Time.realtimeSinceStartup;

            _tweenKey = Guid.NewGuid() + "_toastMove";

            _toastMainContent = DynamicUIManager.ToastContainer.CreateSubContent(new Vector2(
                    Toaster.TOAST_CONTAINER_WIDTH,
                    Toaster.TOAST_HEIGHT + Toaster.DROP_SHADOW_DISTANCE))
                .SetBottomStretch<Content>();
            var dropShadow = _toastMainContent.CreateSubContent(new Vector2(Toaster.TOAST_CONTAINER_WIDTH, Toaster.TOAST_HEIGHT))
                .SetStretch<Content>(topPadding: Toaster.DROP_SHADOW_DISTANCE).EnsureImage().StepIntoImage(i => i.SetMultiplier(18));
            var content = _toastMainContent.CreateSubContent(new Vector2(Toaster.TOAST_CONTAINER_WIDTH, Toaster.TOAST_HEIGHT))
                .SetStretch<Content>(bottomPadding: Toaster.DROP_SHADOW_DISTANCE).EnsureImage().StepIntoImage(i => i.SetMultiplier(18));

            Sprite? s = null;
            switch (level) {
                case LogLevel.Debug:
                    content.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.ToastDebug));
                    dropShadow.StepIntoImage(i => i.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.ToastDebug) * Toaster.DROP_SHADOW_MOD_COLOR));
                    s = SynthesisAssetCollection.GetSpriteByName("wrench-icon");
                    break;
                case LogLevel.Warning:
                    content.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.ToastWarning));
                    dropShadow.StepIntoImage(i => i.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.ToastWarning) * Toaster.DROP_SHADOW_MOD_COLOR));
                    s = SynthesisAssetCollection.GetSpriteByName("warning-icon-white-solid");
                    break;
                case LogLevel.Error:
                    content.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.ToastError));
                    dropShadow.StepIntoImage(i => i.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.ToastError) * Toaster.DROP_SHADOW_MOD_COLOR));
                    s = SynthesisAssetCollection.GetSpriteByName("error-icon-white-solid");
                    break;
                case LogLevel.Info:
                    content.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.ToastInfo));
                    dropShadow.StepIntoImage(i => i.SetColor(ColorManager.GetColor(ColorManager.SynthesisColor.ToastInfo) * Toaster.DROP_SHADOW_MOD_COLOR));
                    s = SynthesisAssetCollection.GetSpriteByName("info-icon-white-solid");
                    break;
            }

            title ??= Enum.GetName(typeof(LogLevel), level) ?? "extra error";
            title = title.ToUpper();

            (float left, float right, float top, float bottom) infoContentPadding = (15f, 15f, 15f, 15f);
            var infoContent = content.CreateSubContent(new Vector2(
                    Toaster.TOAST_CONTAINER_WIDTH - (infoContentPadding.left + infoContentPadding.right),
                    Toaster.TOAST_HEIGHT - (infoContentPadding.top + infoContentPadding.bottom)))
                .SetStretch<Content>(
                    topPadding: infoContentPadding.top,
                    bottomPadding: infoContentPadding.bottom,
                    leftPadding: infoContentPadding.left,
                    rightPadding: infoContentPadding.right);
            var (iconContent, textButtonContent) = infoContent.SplitLeftRight(Toaster.TOAST_HEIGHT - (infoContentPadding.top + infoContentPadding.bottom), 15f);

            var buttonContentPadding = 10f;
            var buttonContentWidth = (textButtonContent.Size.y - buttonContentPadding) / 2f;
            var (textContent, buttonContent) = textButtonContent.SplitLeftRight(textButtonContent.Size.x - (15f + buttonContentWidth), 15f);

            var (topButtonContent, bottomButtonContent) = buttonContent.SplitTopBottom(buttonContent.Size.x, buttonContentPadding);
            topButtonContent.CreateButton("").SetStretch<Button>(topPadding: -5f, bottomPadding: -5f, rightPadding: -5f, leftPadding: -5f)
                .StepIntoImage(i => i.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon")).SetColor(Color.white))
                .AddOnClickedEvent(_ => {
                    closeCallback?.Invoke(this);
                    if (LinkedNode != null)
                        Toaster.RemoveToast(LinkedNode);
                });

            if (optionalCallback != null) {
                bottomButtonContent.CreateButton("").SetStretch<Button>(topPadding: -5f, bottomPadding: -5f, rightPadding: -5f, leftPadding: -5f)
                    .StepIntoImage(i => i.SetSprite(SynthesisAssetCollection.GetSpriteByName("DownArrowIcon")).SetColor(Color.white))
                    .AddOnClickedEvent(_ => {
                        optionalCallback.Invoke(this);
                    });
            }

            var (topLabelContent, bottomLabelContent) = textContent.SplitTopBottom(Toaster.TOAST_HEIGHT / 2 - (infoContentPadding.top + 3.75f), 7.5f);

            if (s != null)
                iconContent.EnsureImage().StepIntoImage(i => i.SetSprite(s).SetColor(Color.white));

            topLabelContent.CreateLabel().SetStretch()
                .SetText(title).SetAutomaticFontSize(true).SetColor(Color.white);
            bottomLabelContent.CreateLabel().SetStretch(topPadding: -3f, bottomPadding: -3f)
                .SetText(message).SetAutomaticFontSize(true).SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
                .SetColor(Color.white).SetWrapping(false).SetOverflowMode(TextOverflowModes.Ellipsis)
                .SetFontMinMaxSize(14f, 18f);
            
            SetActualHeight(-(Toaster.TOAST_HEIGHT + Toaster.TOAST_SPACING));
        }

        public void SetHeight(float height) {
            SynthesisTween.CancelTween(_tweenKey);

            if (_toastMainContent == null || _toastMainContent.RootRectTransform == null)
                return;

            // Skip if close ish
            if (Mathf.Abs(height - _toastMainContent.RootRectTransform.anchoredPosition.y) < 1f
                || (_toastMainContent.RootRectTransform.anchoredPosition.y > Toaster.TOAST_CONTAINER_HEIGHT && height > Toaster.TOAST_CONTAINER_HEIGHT)) {
                SetActualHeight(height);
                return;
            }

            SynthesisTween.MakeTween(_tweenKey,
                _toastMainContent.RootRectTransform.anchoredPosition.y,
                height,
                0.2f,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float)a, (float)b),
                SynthesisTweenScaleFunctions.EaseOutCubic,
                TweenProgress
            );
        }

        private void SetActualHeight(float height) {
            if (_toastMainContent != null) {
                
                var t = (height - (Toaster.TOAST_CONTAINER_HEIGHT - 2 * Toaster.TOAST_HEIGHT)) / Toaster.TOAST_HEIGHT;
                t = Mathf.Clamp(t, 0f, 1f);
                var xPos = Mathf.SmoothStep(0f, (Toaster.TOAST_CONTAINER_WIDTH + 1) - Toaster.TOAST_CONTAINER_OFFSET.x, t);
                
                _toastMainContent.RootRectTransform.anchoredPosition = new Vector2(
                    xPos,
                    height
                );
            }
        }

        private void TweenProgress(SynthesisTween.SynthesisTweenStatus status) {
            SetActualHeight(status.CurrentValue<float>());
        }

        public void Dispose() {
            if (_toastMainContent?.RootGameObject != null) {
                Object.Destroy(_toastMainContent.RootGameObject);
                _toastMainContent = null;
            }
        }
    }
}
