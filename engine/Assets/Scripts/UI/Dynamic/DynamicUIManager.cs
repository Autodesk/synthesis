using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using SynthesisAPI.Utilities;

using Logger = SynthesisAPI.Utilities.Logger;
using Synthesis.Replay;
using Synthesis.Physics;
using SynthesisAPI.EventBus;
using Synthesis.Gizmo;
using Synthesis.Runtime;
using System.Linq;

namespace Synthesis.UI.Dynamic {
    public static class DynamicUIManager {

        public static ModalDynamic ActiveModal { get; private set; }

        private static Dictionary<Type, (PanelDynamic, bool)> _persistentPanels = new Dictionary<Type, (PanelDynamic, bool)>();
        public static bool AnyPanels => _persistentPanels.Count > 0;
        // public static PanelDynamic ActivePanel { get; private set; }
        public static Content _screenSpaceContent = null;
        public static Content ScreenSpaceContent { 
            get {
                if (_screenSpaceContent == null) {
                    _screenSpaceContent = new Content(null, GameObject.Find("UI").transform.Find("ScreenSpace").gameObject, null);
                    SimulationRunner.OnSimKill += () => _screenSpaceContent = null;
                }
                return _screenSpaceContent;
            }
        }
        private static Slider _replaySlider = null;
        public static Slider ReplaySlider {
            get {
                if (_replaySlider == null)
                    _replaySlider = ScreenSpaceContent
                        .CreateSlider(label: "Last 5 seconds", unitSuffix: "s", minValue: -ReplayManager.TimeSpan, maxValue: 0f, currentValue: 0f)
                        .SetBottomStretch<Slider>(leftPadding: 100f, rightPadding: 100f, anchoredY: 50)
                        .SetSlideDirection(UnityEngine.UI.Slider.Direction.LeftToRight)
                        .StepIntoBackgroundImage(i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_ORANGE)))
                        .StepIntoFillImage(i => i.SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK)))
                        .StepIntoTitleLabel(l => l.SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom)
                            .SetFontSize(20).SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK)))
                        .StepIntoValueLabel(l => l.SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom)
                            .SetFontSize(20).SetColor(ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK)));
                    SimulationRunner.OnSimKill += () => { _replaySlider = null; };
                return _replaySlider;
            }
        }
        // public static GameObject ActiveModalGameObject;

        public static bool CreateModal<T>(params object[] args) where T : ModalDynamic {

            CloseAllPanels();
            HideAllPanels();
            GizmoManager.ExitGizmo();
            if (ActiveModal != null)
                CloseActiveModal();
            
            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("dynamic-modal-base"), GameObject.Find("UI").transform.Find("ScreenSpace").Find("ModalContainer"));

            // var c = ColorManager.GetColor("SAMPLE");

            ModalDynamic modal = (ModalDynamic)Activator.CreateInstance(typeof(T), args);
            modal.Create_Internal(unityObject);
            modal.Create();

            ActiveModal = modal;
            EventBus.Push(new ModalCreatedEvent(modal));

            SynthesisAssetCollection.BlurVolumeStatic.weight = 1f;
            PhysicsManager.IsFrozen = true;
            MainHUD.Enabled = false;
            AnalyticsManager.LogEvent(new AnalyticsEvent(category: "ui", action: $"{typeof(T).Name}", label:"create"));
            AnalyticsManager.PostData();
            return true;
        }

        // Currently only going to allow one active panel
        public static bool CreatePanel<T>(bool persistent = false, params object[] args) where T : PanelDynamic {
            
            if (ActiveModal != null)
                return false;

            if (_persistentPanels.ContainsKey(typeof(T)))
                ClosePanel(typeof(T));
            
            // if (ActivePanel != null)
            //     CloseActivePanel();

            var unityObject = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("dynamic-panel-base"), GameObject.Find("UI").transform.Find("ScreenSpace").Find("PanelContainer"));

            // var c = ColorManager.GetColor("SAMPLE");

            PanelDynamic panel = (PanelDynamic)Activator.CreateInstance(typeof(T), args);
            // ActivePanel = panel;
            _persistentPanels[typeof(T)] = (panel, persistent);
            panel.Create_Internal(unityObject);
            bool success = panel.Create();

            Debug.Log($"Panel create: {success}");

            if (!success) {
                ClosePanel<T>();
                return false;
            }

            if (PanelExists(typeof(T)))
                EventBus.Push(new PanelCreatedEvent(panel, persistent));

            AnalyticsManager.LogEvent(new AnalyticsEvent(category: "ui", action: $"{typeof(T).Name}", label:"create"));
            AnalyticsManager.PostData();
            return true;
        }
        
        public static bool CloseActiveModal() {
            if (ActiveModal == null) {
                return false;
            }

            EventBus.Push(new ModalClosedEvent(ActiveModal));

            AnalyticsManager.LogEvent(new AnalyticsEvent(category: "ui", action: $"{ActiveModal.GetType().Name}", label:"create"));
            AnalyticsManager.PostData();

            ActiveModal.Delete();
            ActiveModal.Delete_Internal();

            ActiveModal = null;

            SynthesisAssetCollection.BlurVolumeStatic.weight = 0f;
            PhysicsManager.IsFrozen = false;
            MainHUD.Enabled = true;

            ShowAllPanels();

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

        public static bool ClosePanel<T>() where T : PanelDynamic
            => ClosePanel(typeof(T));

        public static bool ClosePanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;
            EventBus.Push(new PanelClosedEvent(panel));

            AnalyticsManager.LogEvent(new AnalyticsEvent(category: "ui", action: $"{t.Name}", label:"create"));
            AnalyticsManager.PostData();

            panel.Delete();
            panel.Delete_Internal();

            // ActivePanel = null;
            _persistentPanels.Remove(t);
            return true;
        }

        public static bool PanelExists<T>() where T : PanelDynamic
            => PanelExists(typeof(T));

        public static bool PanelExists(Type t)
            => _persistentPanels.ContainsKey(t);

        public static void HideAllPanels() {
            _persistentPanels.ForEach(kvp => HidePanel(kvp.Value.Item1.GetType()));
        }

        public static void HidePanel<T>() where T : PanelDynamic
            => HidePanel(typeof(T));

        public static bool HidePanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;
            if (panel.Hidden)
                return false;

            panel.Hidden = true;
            return true;
        }

        public static void ShowAllPanels() {
            _persistentPanels.ForEach(kvp => ShowPanel(kvp.Value.Item1.GetType()));
        }

        public static bool ShowPanel<T>() where T : PanelDynamic
            => ShowPanel(typeof(T));

        public static bool ShowPanel(Type t) {
            if (!PanelExists(t))
                return false;

            var panel = _persistentPanels[t].Item1;
            if (!panel.Hidden)
                return false;

            panel.Hidden = false;
            return true;
        }

        public static void Update() {
            if (ActiveModal != null)
                ActiveModal.Update();
            _persistentPanels.ForEach(kvp => kvp.Value.Item1.Update());
        }

        public static T ApplyTemplate<T>(this T component, Func<T, T> template) where T : UIComponent
            => template(component);
        public static T ApplyTemplate<T>(this T component, Func<UIComponent, UIComponent> template) where T : UIComponent
            => template(component) as T;
        
        public static Rect GetOffsetRect(this RectTransform trans) {
            var min = new Vector2(trans.anchoredPosition.x + trans.rect.xMin, trans.anchoredPosition.y + trans.rect.yMin);
            var max = new Vector2(trans.anchoredPosition.x + trans.rect.xMax, trans.anchoredPosition.y + trans.rect.yMax);
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
                Panel = panel;
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
