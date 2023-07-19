using System;
using System.Collections.Generic;
using DigitalRuby.Tween;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities.ColorManager;
using Button  = Synthesis.UI.Dynamic.Button;
using Image   = Synthesis.UI.Dynamic.Image;
using UButton = UnityEngine.UI.Button;

#nullable enable

public static class MainHUD {
    private const string COLLAPSE_TWEEN = "collapse";
    private const string EXPAND_TWEEN   = "expand";

    private static Action<ITween<float>> collapseTweenProgress = v => {
        _tabDrawerContent?.SetAnchoredPosition<Content>(
            new Vector2(v.CurrentValue, _tabDrawerContent.RootRectTransform.anchoredPosition.y));
    };

    private const int TAB_DRAWER_WIDTH = 250;
    private const int TAB_DRAWER_X     = 20;

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
                    _tabDrawerContent.RootGameObject.Tween(COLLAPSE_TWEEN,
                        _tabDrawerContent.RootRectTransform.anchoredPosition.x, -TAB_DRAWER_WIDTH - TAB_DRAWER_X, 0.2f,
                        TweenScaleFunctions.CubicEaseOut, collapseTweenProgress);
                } else {
                    TweenFactory.RemoveTweenKey(COLLAPSE_TWEEN, TweenStopBehavior.DoNotModify);
                    _tabDrawerContent.RootGameObject.Tween(EXPAND_TWEEN,
                        _tabDrawerContent.RootRectTransform.anchoredPosition.x, TAB_DRAWER_X, 0.2f,
                        TweenScaleFunctions.CubicEaseOut, collapseTweenProgress);
                }
            }
        }
    }

    private static bool _hasNewRobotListener = false; // In the Unity editor, working with statics can be really weird

    private static Button _accordionButton;
    private static Content _tabDrawerContent;
    private static Image _logoImage;
    private static Content _itemContainer;
    private static Content _topItemContainer;
    private static Content _bottomItemContainer;
    private static Button _spawnButton;
    private static Button _homeButton;

    private static List<(Button button, Image image)> _topDrawerItems    = new List<(Button button, Image image)>();
    private static List<(Button button, Image image)> _bottomDrawerItems = new List<(Button button, Image image)>();

    public enum DrawerPosition {
        Top,
        Bottom
    }

    public static void Setup() {
        _topDrawerItems.Clear();
        _bottomDrawerItems.Clear();
        _accordionButton  = new Button(null, GameObject.Find("MainHUD").transform.Find("Accordion").gameObject, null);
        _tabDrawerContent = new Content(null, GameObject.Find("MainHUD").transform.Find("TabDrawer").gameObject, null);
        _logoImage = new Image(_tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("Logo").gameObject);
        _itemContainer = new Content(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("ItemContainer").gameObject, null);
        _topItemContainer = new Content(
            _itemContainer, _itemContainer.RootGameObject.transform.Find("TopItemContainer").gameObject, null);
        _bottomItemContainer = new Content(
            _itemContainer, _itemContainer.RootGameObject.transform.Find("BottomItemContainer").gameObject, null);
        _spawnButton = new Button(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("SpawnButton").gameObject, null);
        _homeButton = new Button(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("HomeButton").gameObject, null);

        _spawnButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background)
            .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText));
        _homeButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background)
            .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText));

        _accordionButton.Image.SetSprite(
            SynthesisAssetCollection.GetSpriteByName(Collapsed ? "accordion" : "CloseIcon"));
        _accordionButton.OnClicked += (b) => {
            Collapsed = !Collapsed;
            _accordionButton.Image.SetSprite(
                SynthesisAssetCollection.GetSpriteByName(Collapsed ? "accordion" : "CloseIcon"));
        };

        _spawnButton.OnClicked += (b) => { DynamicUIManager.CreateModal<SpawningModal>(); };
        _spawnButton.SetTransition(Selectable.Transition.ColorTint).SetInteractableColors();

        _homeButton.OnClicked += (b) => { DynamicUIManager.CreateModal<ExitSynthesisModal>(); };
        _homeButton.SetTransition(Selectable.Transition.ColorTint).SetInteractableColors();

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
                    AddItemToDrawer("Configure", b => DynamicUIManager.CreateModal<ConfiguringModal>(), index: 1,
                        icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
                }
            });
            _hasNewRobotListener = true;
        }

        _isSetup = true;

        SceneManager.activeSceneChanged += (Scene a, Scene b) => { _isSetup = false; };

        AssignColors(null);

        EventBus.NewTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public static void AddItemToDrawer(
        string title, Action<Button> onClick, int index = -1, Sprite? icon = null, Color? color = null) {
        AddItemToDrawer(title, onClick, DrawerPosition.Top, index, icon, color);
    }

    public static void AddItemToDrawer(string title, Action<Button> onClick, DrawerPosition drawerPosition,
        int index = -1, Sprite? icon = null, Color? color = null) {
        if (!SimulationRunner.InSim)
            return;

        var drawerButtonObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("hud-drawer-item-base"),
            (drawerPosition == DrawerPosition.Top ? _topItemContainer : _bottomItemContainer).RootGameObject.transform);
        drawerButtonObj.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier = 20;

        var drawerButton = new Button(_tabDrawerContent, drawerButtonObj, null);
        drawerButton.Label!.SetText(title).SetFontSize(16);
        drawerButton.AddOnClickedEvent(onClick);
        drawerButton.SetTransition(Selectable.Transition.ColorTint).SetInteractableColors();

        var drawerIcon = new Image(_tabDrawerContent, drawerButtonObj.transform.Find("ItemIcon").gameObject);
        if (icon != null)
            drawerIcon.SetSprite(icon);

        drawerIcon.RootGameObject.transform.localScale = new Vector3(0.65F, 0.65F, 1F);

        switch (drawerPosition) {
            case DrawerPosition.Bottom:
                if (index < 0 || index > _bottomDrawerItems.Count) {
                    _bottomDrawerItems.Add((drawerButton, drawerIcon));
                } else {
                    _bottomDrawerItems.Insert(index, (drawerButton, drawerIcon));
                }
                break;
            default:
            case DrawerPosition.Top:
                if (index < 0 || index > _topDrawerItems.Count) {
                    _topDrawerItems.Add((drawerButton, drawerIcon));
                } else {
                    _topDrawerItems.Insert(index, (drawerButton, drawerIcon));
                }
                break;
        }

        UpdateDrawerSizing();
        AssignColors(null);
    }

    public static void RemoveItemFromDrawer(string title) {
        if (!SimulationRunner.InSim)
            return;

        var index = _topDrawerItems.FindIndex(0, _topDrawerItems.Count, x => x.button.Label.Text == title);
        if (index == -1)
            return;

        GameObject.Destroy(_topDrawerItems[index].button.RootGameObject);
        _topDrawerItems.RemoveAt(index);

        UpdateDrawerSizing();
    }

    public static void UpdateDrawerSizing() {
        int LOGO_BOTTOM_Y       = 50;
        int LOGO_BUTTON_SPACING = 5;
        int BUTTON_HEIGHT       = 60;
        int SPACING             = 15;
        int ITEM_HEIGHT         = 40;
        int BOTTOM_PADDING      = 40;

        for (int i = 0; i < _topDrawerItems.Count; i++) {
            _topDrawerItems[i]
                .button
                .SetTopStretch<Button>(
                    anchoredY: SPACING + i * (ITEM_HEIGHT + SPACING), leftPadding: SPACING, rightPadding: SPACING)
                .StepIntoLabel(l => l.SetStretch<Label>(leftPadding: 55));
        }
        for (int i = 0; i < _bottomDrawerItems.Count; i++) {
            _bottomDrawerItems[i]
                .button
                .SetTopStretch<Button>(
                    anchoredY: SPACING + i * (ITEM_HEIGHT + SPACING), leftPadding: SPACING, rightPadding: SPACING)
                .StepIntoLabel(l => l.SetStretch<Label>(leftPadding: 55));
        }
        _itemContainer.SetTopStretch<Content>(anchoredY: LOGO_BOTTOM_Y + LOGO_BUTTON_SPACING + BUTTON_HEIGHT + SPACING,
            leftPadding: SPACING, rightPadding: SPACING);
        int topItemsHeight    = SPACING + _topDrawerItems.Count * (ITEM_HEIGHT + SPACING);
        int bottomItemsHeight = SPACING + _bottomDrawerItems.Count * (ITEM_HEIGHT + SPACING);
        _topItemContainer.SetHeight<Content>(topItemsHeight);
        _topItemContainer.SetTopStretch<Content>(anchoredY: 0);
        _bottomItemContainer.SetHeight<Content>(bottomItemsHeight);
        _bottomItemContainer.SetTopStretch<Content>(anchoredY: topItemsHeight + SPACING);
        int itemsHeight = topItemsHeight + SPACING + bottomItemsHeight;
        _itemContainer.SetHeight<Content>(itemsHeight);
        _tabDrawerContent.SetHeight<Content>(LOGO_BOTTOM_Y + LOGO_BUTTON_SPACING + BUTTON_HEIGHT + SPACING +
                                             itemsHeight + SPACING + BUTTON_HEIGHT + BOTTOM_PADDING);
    }

    public static void Delete() {
        EventBus.RemoveTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public static void AssignColors(IEvent e) {
        _tabDrawerContent.Image!.SetColor(
            ColorManager.SynthesisColor.InteractiveElementLeft, ColorManager.SynthesisColor.InteractiveElementRight);

        _spawnButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background);
        _homeButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background);

        _topItemContainer.SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background);
        _bottomItemContainer.SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background);

        _topDrawerItems.ForEach(x => {
            x.button.Label!.SetColor(ColorManager.SynthesisColor.MainText);
            x.button.Image.SetColor(ColorManager.SynthesisColor.Background);
            x.image.SetColor(ColorManager.SynthesisColor.MainText);
        });
        _bottomDrawerItems.ForEach(x => {
            x.button.Label!.SetColor(ColorManager.SynthesisColor.MainText);
            x.button.Image.SetColor(ColorManager.SynthesisColor.Background);
            x.image.SetColor(ColorManager.SynthesisColor.MainText);
        });
    }
}
