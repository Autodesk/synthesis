using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Dynamic;

using UButton = UnityEngine.UI.Button;
using System.Linq;
using Synthesis.UI;
using DigitalRuby.Tween;
using SynthesisAPI.EventBus;
using UnityEngine.SceneManagement;
using Synthesis.Runtime;

#nullable enable

public static class MainHUD {
    private const string COLLAPSE_TWEEN = "collapse";
    private const string EXPAND_TWEEN   = "expand";

    private static Action<ITween<float>> collapseTweenProgress =
        v => { _tabDrawerContent?.SetWidth<Content>(v.CurrentValue); };

    private static bool _isSetup = false;
    private static bool _enabled = true;
    public static bool Enabled {
        get => _enabled;
        set {
            if (!_isSetup)
                return;

            if (_enabled != value) {
                _enabled = value;
                if (_enabled) {
                    _tabDrawerContent.RootGameObject.SetActive(true);
                } else {
                    Collapsed = true;
                    _tabDrawerContent.RootGameObject.SetActive(false);
                }
            }
        }
    }

    private static bool _collapsed = false;
    public static bool Collapsed {
        get => _collapsed;
        set {
            if (!_isSetup)
                return;

            if (_collapsed != value) {
                _collapsed = value;
                if (_collapsed) {
                    TweenFactory.RemoveTweenKey(EXPAND_TWEEN, TweenStopBehavior.DoNotModify);
                    _tabDrawerContent.RootGameObject.Tween(COLLAPSE_TWEEN, _tabDrawerContent.Size.x, 20 + 15 + 40 + 15,
                        0.2f, TweenScaleFunctions.CubicEaseOut, collapseTweenProgress);
                } else {
                    TweenFactory.RemoveTweenKey(COLLAPSE_TWEEN, TweenStopBehavior.DoNotModify);
                    _tabDrawerContent.RootGameObject.Tween(EXPAND_TWEEN, _tabDrawerContent.Size.x, 20 + 15 + 200 + 15,
                        0.2f, TweenScaleFunctions.CubicEaseOut, collapseTweenProgress);
                }
            }
        }
    }

    private static bool _hasNewRobotListener = false; // In the Unity editor, working with statics can be really weird

    private static Content _tabDrawerContent;
    private static Button _expandDrawerButton;

    private static List<(Button button, Image image)> _drawerItems = new List<(Button button, Image image)>();

    public static void Setup() {
        _drawerItems.Clear();
        _tabDrawerContent = new Content(null, GameObject.Find("MainHUD").transform.Find("TabDrawer").gameObject, null);
        _tabDrawerContent.Image!.SetColor(ColorManager.SYNTHESIS_BLACK);
        _expandDrawerButton = new Button(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("ExpandButton").gameObject, null);
        _expandDrawerButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK));
        _expandDrawerButton.AddOnClickedEvent(b => MainHUD.Collapsed = !MainHUD.Collapsed);
        var expandIcon = new Image(null, _expandDrawerButton.RootGameObject.transform.Find("Icon").gameObject);
        expandIcon.SetColor(ColorManager.SYNTHESIS_ICON);

        // Setup default HUD
        // MOVED TO PRACTICE MODE

        if (!_hasNewRobotListener) {
            EventBus.NewTypeListener<RobotSimObject.PossessionChangeEvent>(e => {
                var robotEvent = e as RobotSimObject.PossessionChangeEvent;

                if (robotEvent == null)
                    throw new Exception("Event type parsed incorrectly. Shouldn't ever happen");

                if (robotEvent.NewBot == string.Empty) {
                    RemoveItemFromDrawer("Configure");
                } else if (robotEvent.OldBot == string.Empty) {
                    MainHUD.AddItemToDrawer("Configure", b => DynamicUIManager.CreateModal<ConfiguringModal>(),
                        index: 1, icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
                }
            });
            _hasNewRobotListener = true;
        }

        _isSetup = true;

        SceneManager.activeSceneChanged += (Scene a, Scene b) => { _isSetup = false; };
    }

    public static void AddItemToDrawer(
        string title, Action<Button> onClick, int index = -1, Sprite? icon = null, Color? color = null) {
        if (!SimulationRunner.InSim)
            return;

        var drawerButtonObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("hud-drawer-item-base"),
            _tabDrawerContent.RootGameObject.transform.Find("ItemContainer"));
        var drawerButton    = new Button(_tabDrawerContent, drawerButtonObj, null);
        drawerButton.Label!.SetText(title).SetColor(ColorManager.SYNTHESIS_WHITE);
        drawerButton.Image.SetColor(new Color(1, 1, 1, 0));
        drawerButton.AddOnClickedEvent(onClick);
        var drawerIcon = new Image(_tabDrawerContent, drawerButtonObj.transform.Find("ItemIcon").gameObject);
        if (icon != null) {
            drawerIcon.SetSprite(icon);
            if (color.HasValue) {
                drawerIcon.SetColor(color.Value);
            } else {
                drawerIcon.SetColor(ColorManager.SYNTHESIS_ORANGE);
            }
        } else {
            if (color.HasValue) {
                drawerIcon.SetColor(color.Value);
            } else {
                drawerIcon.SetColor(ColorManager.SYNTHESIS_ORANGE);
            }
        }
        if (index < 0 || index > _drawerItems.Count) {
            _drawerItems.Add((drawerButton, drawerIcon));
        } else {
            _drawerItems.Insert(index, (drawerButton, drawerIcon));
        }

        UpdateDrawerSizing();
    }

    public static void RemoveItemFromDrawer(string title) {
        if (!SimulationRunner.InSim)
            return;

        var index = _drawerItems.FindIndex(0, _drawerItems.Count, x => x.button.Label.Text == title);
        if (index == -1)
            return;

        GameObject.Destroy(_drawerItems[index].button.RootGameObject);
        _drawerItems.RemoveAt(index);

        UpdateDrawerSizing();
    }

    public static void UpdateDrawerSizing() {
        for (int i = 0; i < _drawerItems.Count; i++) {
            _drawerItems[i].button.SetTopStretch<Button>(anchoredY: i * 55);
        }
        _tabDrawerContent.SetHeight<Content>((_drawerItems.Count * 55) + 70);
    }
}
