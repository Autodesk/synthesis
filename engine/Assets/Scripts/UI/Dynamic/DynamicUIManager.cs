using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Gizmo;
using Synthesis.Physics;
using Synthesis.Replay;
using Synthesis.Runtime;
using Analytics;
using Synthesis.Util;
using Utilities.ColorManager;
using SynthesisAPI.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.UI.Dynamic {
    public static class DynamicUIManager {
        public const float MODAL_TWEEN_DURATION = 0.2f;
        public const float PANEL_TWEEN_DURATION = 0.2f;
        
        public static ModalDynamic ActiveModal { get; private set; }

        private static Dictionary<Type, (PanelDynamic, bool)> _persistentPanels =
            new Dictionary<Type, (PanelDynamic, bool)>();
        public static bool AnyPanels              => _persistentPanels.Count > 0;
        public static Content _screenSpaceContent  = null;
        public static Content ScreenSpaceContent {
            get {
                if (_screenSpaceContent == null) {
                    _screenSpaceContent =
                        new Content(null, GameObject.Find("UI").transform.Find("ScreenSpace").gameObject, null);
                    SimulationRunner.OnSimKill += () => _screenSpaceContent = null;
                }
                return _screenSpaceContent;
            }
        }
        public static Content _subScreenSpaceContent = null;
        public static Content SubScreenSpaceContent {
            get {
                if (_subScreenSpaceContent == null) {
                    _subScreenSpaceContent =
                        new Content(null, GameObject.Find("UI").transform.Find("SubScreenSpace").gameObject, null);
                    SimulationRunner.OnSimKill += () => _subScreenSpaceContent = null;
                }
                return _subScreenSpaceContent;
            }
        }
        private static Slider _replaySlider = null;
        public static Slider ReplaySlider {
            get {
                if (_replaySlider == null)
                    _replaySlider =
                        ScreenSpaceContent
                            .CreateSlider(label: "Last 5 seconds", unitSuffix: "s", minValue: -ReplayManager.TimeSpan,
                                maxValue: 0f, currentValue: 0f)
                            .SetBottomStretch<Slider>(leftPadding: 100f, rightPadding: 100f, anchoredY: 50)
                            .SetSlideDirection(UnityEngine.UI.Slider.Direction.LeftToRight)
                            .StepIntoBackgroundImage(
                                i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementSolid))
                            .StepIntoFillImage(i => i.SetColor(ColorManager.SynthesisColor.Background))
                            .StepIntoTitleLabel(l => l.SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom)
                                                         .SetFontSize(20)
                                                         .SetColor(ColorManager.SynthesisColor.Background))
                            .StepIntoValueLabel(l => l.SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom)
                                                         .SetFontSize(20)
                                                         .SetColor(ColorManager.SynthesisColor.Background));
                SimulationRunner.OnSimKill += () => { _replaySlider = null; };
                return _replaySlider;
            }
        }

        private static Content _toastContainer = null;
        public static Content ToastContainer {
            get {
                if (_toastContainer == null || _toastContainer.RootGameObject == null) {
                    _toastContainer = SubScreenSpaceContent
                                          .CreateSubContent(new Vector2(
                                              Toaster.TOAST_CONTAINER_WIDTH, Toaster.TOAST_CONTAINER_HEIGHT))
                                          .SetAnchors<Content>(new Vector2(1, 0), new Vector2(1, 0))
                                          .SetPivot<Content>(new Vector2(1, 0))
                                          .SetAnchoredPosition<Content>(Toaster.TOAST_CONTAINER_OFFSET)
                                          .EnsureImage()
                                          .StepIntoImage(i => i.SetColor(new Color(0.1f, 0.1f, 0.1f, 0f)));
                    _toastContainer.RootGameObject.AddComponent<RectMask2D>();
                }
                SimulationRunner.OnSimKill += () => { _toastContainer = null; };
                return _toastContainer;
            }
        }

        public static bool CreateModal<T>(params object[] args)
            where T : ModalDynamic {
            CloseAllPanels();
            HideAllPanels();
            GizmoManager.ExitGizmo();
            if (ActiveModal != null)
                CloseActiveModal();

            return CreateModal_Internal<T>(args);
        }

        public static bool CreateModalWithoutOverwrite<T>(params object[] args)
            where T : ModalDynamic {
            if (_persistentPanels.Count > 0)
                return false;
            if (ActiveModal != null)
                return false;

            return CreateModal_Internal<T>(args);
        }

        private static bool CreateModal_Internal<T>(params object[] args)
            where T : ModalDynamic {
            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("dynamic-modal-base"),
                GameObject.Find("UI").transform.Find("ScreenSpace").Find("ModalContainer"));

            ModalDynamic modal = (ModalDynamic) Activator.CreateInstance(typeof(T), args);
            modal.Create_Internal(unityObject);
            modal.Create();

            ActiveModal = modal;
            EventBus.Push(new ModalCreatedEvent(modal));

            SynthesisAssetCollection.BlurVolumeStatic.weight = 1f;
            PhysicsManager.IsFrozen                          = true;
            MainHUD.Enabled                                  = false;
            SubScreenSpaceContent.RootGameObject.SetActive(false);

            
            string tweenKey = Guid.NewGuid() + "_modalOpen";
            SynthesisTween.MakeTween(tweenKey, 0f, 1f, MODAL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                unityObject.transform.localScale = Vector3.one * status.CurrentValue<float>();
            }
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.ModalCreated, ("UIType", typeof(T).Name));
            return true;
        }

        // Currently only going to allow one active panel
        public static bool CreatePanel<T>(bool persistent = false, params object[] args)
            where T : PanelDynamic {
            if (_persistentPanels.ContainsKey(typeof(T)))
                ClosePanel(typeof(T));

            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("dynamic-panel-base"),
                GameObject.Find("UI").transform.Find("ScreenSpace").Find("PanelContainer"));

            PanelDynamic panel           = (PanelDynamic) Activator.CreateInstance(typeof(T), args);
            _persistentPanels[typeof(T)] = (panel, persistent);
            panel.Create_Internal(unityObject);
            bool success = panel.Create();

            if (!success) {
                ClosePanel<T>();
                return false;
            }

            if (PanelExists(typeof(T)))
                EventBus.Push(new PanelCreatedEvent(panel, persistent));

            
            string tweenKey = Guid.NewGuid() + "_panelOpen";
            
            Vector3 endPosition = unityObject.transform.localPosition;
            float tweenStart = (!panel.TweenFromBottom)
                ? (endPosition.x+((RectTransform)unityObject.transform).sizeDelta.x)
                : (endPosition.y-((RectTransform)unityObject.transform).sizeDelta.y);
            
            float tweenEnd = (!panel.TweenFromBottom)
                ? (endPosition.x)
                : (endPosition.y);
            
            SynthesisTween.MakeTween(tweenKey, tweenStart, tweenEnd, PANEL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                Vector3 position = (!panel.TweenFromBottom) ?
                    new Vector3(status.CurrentValue<float>(), endPosition.y, endPosition.z)
                    : new Vector3(endPosition.x, status.CurrentValue<float>(), endPosition.z);
                unityObject.transform.localPosition = position;
            }
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.PanelCreated, ("UIType", typeof(T).Name));
            return true;
        }

        public static bool CloseActiveModal() {
            var modal = ActiveModal;

            if (modal == null) {
                return false;
            }

            string tweenKey = Guid.NewGuid() + "_modalClose";
            SynthesisTween.MakeTween(tweenKey, 1f, 0f, MODAL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                if (modal.UnityObject == null) {
                    TweenFinished();
                    return;
                }

                modal.UnityObject.transform.localScale = Vector3.one * status.CurrentValue<float>();
                 
                if (status.CurrentProgress >= 1f)
                    TweenFinished();
            }

            void TweenFinished() {
                SynthesisTween.CancelTween(tweenKey);
                
                EventBus.Push(new ModalClosedEvent(modal));
                
                modal.Delete();
                modal.Delete_Internal();
                
                // Unfreeze physics no matter what because it has a counter
                PhysicsManager.IsFrozen = false; 

                if (modal == ActiveModal) {
                    ActiveModal = null;

                    SynthesisAssetCollection.BlurVolumeStatic.weight = 0f;
                    MainHUD.Enabled = true;
                    SubScreenSpaceContent.RootGameObject.SetActive(true);

                    ShowAllPanels();
                }
                
            }
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.ActiveModalClosed, ("UIType", modal.GetType().Name));
            return true;
        }

        public static void CloseAllPanels(bool closePersistent = false) {
            var panels = new List<Type>();
            if (!closePersistent)
                _persistentPanels.Where(kvp => !kvp.Value.Item2).ForEach(kvp => panels.Add(kvp.Value.Item1.GetType()));
            else
                _persistentPanels.ForEach(kvp => panels.Add(kvp.Value.Item1.GetType()));

            panels.ForEach(x => ClosePanel(x));
        }

        public static bool ClosePanel<T>()
            where T : PanelDynamic => ClosePanel(typeof(T));

  public static bool ClosePanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;

            GameObject unityObject = panel.UnityObject;
            string tweenKey = Guid.NewGuid() + "_panelClose";
            Vector3 endPosition = unityObject.transform.localPosition;

            float tweenEnd = (!panel.TweenFromBottom)
                ? (endPosition.x+((RectTransform)unityObject.transform).sizeDelta.x)
                : (endPosition.y-((RectTransform)unityObject.transform).sizeDelta.y);
            
            float tweenStart = (!panel.TweenFromBottom)
                ? (endPosition.x)
                : (endPosition.y);
            
            SynthesisTween.MakeTween(tweenKey, tweenStart, tweenEnd, PANEL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                if (unityObject == null) {
                    TweenFinished();
                    return;
                }
                
                Vector3 position = (!panel.TweenFromBottom) ?
                    new Vector3(status.CurrentValue<float>(), endPosition.y, endPosition.z)
                    : new Vector3(endPosition.x, status.CurrentValue<float>(), endPosition.z);
                unityObject.transform.localPosition = position;

                if (status.CurrentProgress >= 1f)
                    TweenFinished();
            }

            void TweenFinished() {
                EventBus.Push(new PanelClosedEvent(panel));

                panel.Delete();
                panel.Delete_Internal();

                _persistentPanels.Remove(t);
            }
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.PanelClosed, ("UIType", t.Name));
            return true;
        }

        public static bool PanelExists<T>()
            where T : PanelDynamic {
            return PanelExists(typeof(T));
        }

        public static bool PanelExists(Type t) {
            return _persistentPanels.ContainsKey(t);
        }

        public static T GetPanel<T>()
            where T : PanelDynamic {
            if (!PanelExists<T>())
                return null;
            return (T) _persistentPanels[typeof(T)].Item1;
        }

        public static void HideAllPanels() {
            _persistentPanels.ForEach(kvp => HidePanel(kvp.Value.Item1.GetType()));
        }

        public static void HidePanel<T>()
            where T : PanelDynamic {
            HidePanel(typeof(T));
        }

        public static bool HidePanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;
            if (panel.Hidden)
                return false;
            
            GameObject unityObject = panel.UnityObject;
            string tweenKey = Guid.NewGuid() + "_panelClose";
            Vector3 endPosition = unityObject.transform.localPosition;

            float tweenEnd = (!panel.TweenFromBottom)
                ? (endPosition.x+((RectTransform)unityObject.transform).sizeDelta.x)
                : (endPosition.y-((RectTransform)unityObject.transform).sizeDelta.y);
            
            float tweenStart = (!panel.TweenFromBottom)
                ? (endPosition.x)
                : (endPosition.y);
            
            SynthesisTween.MakeTween(tweenKey, tweenStart, tweenEnd, PANEL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                if (unityObject == null) {
                    TweenFinished();
                    return;
                }
                
                Vector3 position = (!panel.TweenFromBottom) ?
                    new Vector3(status.CurrentValue<float>(), endPosition.y, endPosition.z)
                    : new Vector3(endPosition.x, status.CurrentValue<float>(), endPosition.z);
                unityObject.transform.localPosition = position;

                if (status.CurrentProgress >= 1f)
                    TweenFinished();
            }

            void TweenFinished() {
                panel.Hidden = true;
            }

            return true;
        }

        public static void ShowAllPanels() {
            _persistentPanels.ForEach(kvp => ShowPanel(kvp.Value.Item1.GetType()));
        }

        public static bool ShowPanel<T>()
            where T : PanelDynamic {
            return ShowPanel(typeof(T));
        }

        public static bool ShowPanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;
            if (!panel.Hidden)
                return false;
            
            string tweenKey = Guid.NewGuid() + "_panelOpen";

            GameObject unityObject = panel.UnityObject;
            Vector3 endPosition = unityObject.transform.localPosition;
            float tweenStart = (!panel.TweenFromBottom)
                ? (endPosition.x+((RectTransform)unityObject.transform).sizeDelta.x)
                : (endPosition.y-((RectTransform)unityObject.transform).sizeDelta.y);
            
            float tweenEnd = (!panel.TweenFromBottom)
                ? (endPosition.x)
                : (endPosition.y);
            
            SynthesisTween.MakeTween(tweenKey, tweenStart, tweenEnd, PANEL_TWEEN_DURATION,
                (t, a, b) => SynthesisTweenInterpolationFunctions.FloatInterp(t, (float) a, (float) b),
                SynthesisTweenScaleFunctions.EaseOutCubic, TweenCallback);

            void TweenCallback(SynthesisTween.SynthesisTweenStatus status) {
                panel.Hidden = false;
            }

            return true;
        }

        public static void Update() {
            if (ActiveModal != null)
                ActiveModal.Update();
            _persistentPanels.ForEach(kvp => kvp.Value.Item1.Update());
        }

        public static T ApplyTemplate<T>(this T component, Func<T, T> template)
            where T : UIComponent {
            return template(component);
        }

        public static T ApplyTemplate<T>(this T component, Func<UIComponent, UIComponent> template)
            where T : UIComponent {
            return template(component) as T;
        }

        public static Rect GetOffsetRect(this RectTransform trans) {
            var min =
                new Vector2(trans.anchoredPosition.x + trans.rect.xMin, trans.anchoredPosition.y + trans.rect.yMin);
            var max =
                new Vector2(trans.anchoredPosition.x + trans.rect.xMax, trans.anchoredPosition.y + trans.rect.yMax);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public struct ModalCreatedEvent : IEvent {
            public ModalDynamic Modal;

            public ModalCreatedEvent(ModalDynamic modal) {
                Modal = modal;
            }
        }

        public struct PanelCreatedEvent : IEvent {
            public PanelDynamic Panel;
            public bool IsPersistent;

            public PanelCreatedEvent(PanelDynamic panel, bool isPersistent) {
                Panel        = panel;
                IsPersistent = isPersistent;
            }
        }

        public struct ModalClosedEvent : IEvent {
            public ModalDynamic Modal;

            public ModalClosedEvent(ModalDynamic modal) {
                Modal = modal;
            }
        }

        public struct PanelClosedEvent : IEvent {
            public PanelDynamic Panel;

            public PanelClosedEvent(PanelDynamic panel) {
                Panel = panel;
            }
        }
    }
}
