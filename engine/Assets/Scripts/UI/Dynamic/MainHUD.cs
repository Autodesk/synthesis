using System;
using System.Collections.Generic;
using DigitalRuby.Tween;
using Modes.MatchMode;
using Synthesis.Gizmo;
using Synthesis.Physics;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities.ColorManager;
using Button  = Synthesis.UI.Dynamic.Button;
using Image   = Synthesis.UI.Dynamic.Image;
using Logger  = SynthesisAPI.Utilities.Logger;
using Object  = UnityEngine.Object;
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
                _accordionButton.Image.SetSprite(
                    SynthesisAssetCollection.GetSpriteByName(_collapsed ? "accordion" : "CloseIcon"));
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
    public static bool isConfig       = false;
    public static bool isMatchFreeCam = false;

    private static RobotSimObject? _configRobot = RobotSimObject.GetCurrentlyPossessedRobot();
    public static RobotSimObject? ConfigRobot {
        get => _configRobot;
        set {
            _configRobot = value;
            if (value == null) {
                RemoveItemFromDrawer("Configure");
            } else if (!isConfig && !DrawerTitles.Contains("Configure")) {
                AddItemToDrawer(
                    "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
            }
        }
    }

    private static Content _tabDrawerContent;
    private static Image _logoImage;
    private static Content _itemContainer;
    private static Content _topItemContainer;
    private static Content _bottomItemContainer;
    private static Button _spawnButton;
    private static Image _spawnIcon;
    private static Button _homeButton;
    private static Image _homeIcon;

    private static Action<Button> _backCallback;
    private static Action<Button> _spawnCallback;

    private static List<(Button button, Image image)> _topDrawerItems    = new List<(Button button, Image image)>();
    private static List<(Button button, Image image)> _bottomDrawerItems = new List<(Button button, Image image)>();

    public enum DrawerPosition {
        Top,
        Bottom
    }

    public static List<string> DrawerTitles = new List<string>();

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
        _spawnIcon  = new Image(_spawnButton, _spawnButton.RootGameObject.transform.Find("PlusIcon").gameObject);
        _homeButton = new Button(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("HomeButton").gameObject, null);
        _homeIcon = new Image(_homeButton, _homeButton.RootGameObject.transform.Find("HomeIcon").gameObject);

        _spawnButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background)
            .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText));
        _homeButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background)
            .StepIntoLabel(l => l.SetColor(ColorManager.SynthesisColor.MainText));

        _accordionButton.OnClicked += (b) => { Collapsed = !Collapsed; };

        _spawnButton.SetTransition(Selectable.Transition.ColorTint).SetInteractableColors();

        _homeButton.OnClicked += (b) => { DynamicUIManager.CreateModal<ExitSynthesisModal>(); };
        _homeButton.SetTransition(Selectable.Transition.ColorTint).SetInteractableColors();

        _spawnCallback = b => { DynamicUIManager.CreateModal<SpawningModal>(); };
        _backCallback  = b => { LeaveConfig(); };

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
                    AddItemToDrawer(
                        "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));
                }
            });
            _hasNewRobotListener = true;
        }

        _isSetup = true;

        SceneManager.activeSceneChanged += (Scene a, Scene b) => {
            _isSetup  = false;
            Collapsed = true;
        };

        UpdateDrawerSizing();
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

        var drawerButtonObj = Object.Instantiate(SynthesisAssetCollection.GetUIPrefab("hud-drawer-item-base"),
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

        DrawerTitles.Add(title);

        UpdateDrawerSizing();
        AssignColors(null);
    }

    public static void RemoveAllItemsFromDrawer() {
        while (DrawerTitles.Count > 0)
            RemoveItemFromDrawer(DrawerTitles[0]);
    }

    public static void RemoveItemFromDrawer(string title) {
        if (!SimulationRunner.InSim)
            return;

        DrawerTitles.Remove(title);

        var index = _topDrawerItems.FindIndex(0, _topDrawerItems.Count, x => x.button.Label.Text == title);
        if (index == -1) {
            index = _bottomDrawerItems.FindIndex(0, _bottomDrawerItems.Count, x => x.button.Label.Text == title);
            if (index == -1)
                return;
            Object.Destroy(_bottomDrawerItems[index].button.RootGameObject);
            _bottomDrawerItems.RemoveAt(index);
        } else {
            Object.Destroy(_topDrawerItems[index].button.RootGameObject);
            _topDrawerItems.RemoveAt(index);
        }

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
        _itemContainer.SetTopStretch<Content>(anchoredY: LOGO_BOTTOM_Y + LOGO_BUTTON_SPACING + (_spawnButton.RootGameObject.activeSelf ? BUTTON_HEIGHT + SPACING : 0),
            leftPadding: SPACING, rightPadding: SPACING);
        // all the zero checks handle layout when no tray items have been added
        int topItemsHeight = _topDrawerItems.Count == 0 ? 0 : SPACING + _topDrawerItems.Count * (ITEM_HEIGHT + SPACING);
        int bottomItemsHeight =
            _bottomDrawerItems.Count == 0 ? 0 : SPACING + _bottomDrawerItems.Count * (ITEM_HEIGHT + SPACING);
        _topItemContainer.SetHeight<Content>(topItemsHeight);
        _topItemContainer.SetTopStretch<Content>(anchoredY: 0);
        _bottomItemContainer.SetHeight<Content>(bottomItemsHeight);
        _bottomItemContainer.SetTopStretch<Content>(anchoredY: topItemsHeight > 0 ? topItemsHeight + SPACING : 0);
        int itemsHeight =
            topItemsHeight == 0 && bottomItemsHeight == 0 ? 0 : topItemsHeight + SPACING + bottomItemsHeight;
        _itemContainer.SetHeight<Content>(itemsHeight);
        int itemsHeightWithSpacing =
            itemsHeight == 0 ? SPACING : SPACING + itemsHeight + (bottomItemsHeight == 0 ? 0 : SPACING);
        if (!_spawnButton.RootGameObject.activeSelf) itemsHeightWithSpacing -= SPACING;
        _tabDrawerContent.SetHeight<Content>(LOGO_BOTTOM_Y + LOGO_BUTTON_SPACING + 
                                             (_spawnButton.RootGameObject.activeSelf ? BUTTON_HEIGHT : 0) +
                                             itemsHeightWithSpacing + BUTTON_HEIGHT + BOTTOM_PADDING);
    }

    public static void Delete() {
        EventBus.RemoveTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public static void AssignColors(IEvent e) {
        _tabDrawerContent.Image!.SetColor(
            ColorManager.SynthesisColor.InteractiveElementLeft, ColorManager.SynthesisColor.InteractiveElementRight);

        _accordionButton.Image.SetColor(ColorManager.SynthesisColor.Icon);

        _spawnButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background);
        _spawnIcon.SetColor(ColorManager.SynthesisColor.Icon);
        _homeButton.SetBackgroundColor<Button>(ColorManager.SynthesisColor.Background);
        _homeIcon.SetColor(ColorManager.SynthesisColor.Icon);

        _topItemContainer.SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background);
        _bottomItemContainer.SetBackgroundColor<Content>(ColorManager.SynthesisColor.Background);

        _topDrawerItems.ForEach(x => {
            x.button.Label!.SetColor(ColorManager.SynthesisColor.MainText);
            x.button.Image.SetColor(ColorManager.SynthesisColor.Background);
            x.image.SetColor(ColorManager.SynthesisColor.Icon);
        });
        _bottomDrawerItems.ForEach(x => {
            x.button.Label!.SetColor(ColorManager.SynthesisColor.MainText);
            x.button.Image.SetColor(ColorManager.SynthesisColor.Background);
            x.image.SetColor(ColorManager.SynthesisColor.Icon);
        });
    }

    public static void SetUpPractice() {
        RemoveAllItemsFromDrawer();

        _spawnButton.OnClicked -= _backCallback;
        _spawnButton.OnClicked += _spawnCallback;
        
        _spawnButton.RootGameObject.SetActive(true);
        _spawnIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("plus"));
        _spawnButton.Label.SetText("Spawn Asset");
        
        AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("settings"));
        AddItemToDrawer("View", b => DynamicUIManager.CreateModal<ChangeViewModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("search"));
        AddItemToDrawer("MultiBot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("multibot"));
        AddItemToDrawer("Controls", b => DynamicUIManager.CreateModal<ChangeInputsModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("xbox_controller"));
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty && !DrawerTitles.Contains("Configure"))
            AddItemToDrawer(
                "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        AddItemToDrawer("Download Asset", b => DynamicUIManager.CreateModal<DownloadAssetModal>(),
            drawerPosition: DrawerPosition.Bottom, icon: SynthesisAssetCollection.GetSpriteByName("download"));
        AddItemToDrawer("DriverStation",
            b => DynamicUIManager.CreatePanel<BetaWarningPanel>(
                false, (Action) (() => DynamicUIManager.CreatePanel<DriverStationPanel>(true))),
            drawerPosition: DrawerPosition.Bottom, icon: SynthesisAssetCollection.GetSpriteByName("driverstation"));
        AddItemToDrawer("Scoring Zones", b => {
            if (FieldSimObject.CurrentField == null) {
                Logger.Log("No field loaded!", LogLevel.Info);
            } else {
                if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                    DynamicUIManager.CreatePanel<ScoringZonesPanel>();
            }
        }, drawerPosition: DrawerPosition.Bottom);

        PhysicsManager.IsFrozen = false;
    }

    public static void SetUpMatch() {
        RemoveAllItemsFromDrawer();
        
        _spawnButton.RootGameObject.SetActive(false);
        
        AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("settings"));
        AddItemToDrawer("View", b => DynamicUIManager.CreateModal<ChangeViewModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("search"));
        AddItemToDrawer("MultiBot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("multibot"));
        AddItemToDrawer("Controls", b => DynamicUIManager.CreateModal<ChangeInputsModal>(),
            drawerPosition: DrawerPosition.Top, icon: SynthesisAssetCollection.GetSpriteByName("xbox_controller"));
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty && !DrawerTitles.Contains("Configure"))
            AddItemToDrawer(
                "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        AddItemToDrawer("DriverStation",
            b => DynamicUIManager.CreatePanel<BetaWarningPanel>(
                false, (Action) (() => DynamicUIManager.CreatePanel<DriverStationPanel>(true))),
            drawerPosition: DrawerPosition.Bottom, icon: SynthesisAssetCollection.GetSpriteByName("driverstation"));
        AddItemToDrawer("Scoring Zones", b => {
            if (FieldSimObject.CurrentField == null) {
                Logger.Log("No field loaded!", LogLevel.Info);
            } else {
                if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                    DynamicUIManager.CreatePanel<ScoringZonesPanel>();
            }
        }, drawerPosition: DrawerPosition.Bottom);

        if ((MatchStateMachine.Instance.CurrentState.StateName is MatchStateMachine.StateName.RobotPositioning) &&
            Camera.main != null) {
            FreeCameraMode camMode = CameraController.CameraModes["Freecam"] as FreeCameraMode;
            Camera.main.GetComponent<CameraController>().CameraMode = camMode;
            var location                                            = new Vector3(0, 6, -8);
            camMode.SetTransform(location,
                Quaternion.LookRotation(-location.normalized, Vector3.Cross(-location.normalized, Vector3.right)));
        }
    }

    public static void SetUpConfig() {
        isConfig = true;

        _spawnButton.OnClicked -= _spawnCallback;
        _spawnButton.OnClicked += _backCallback;

        _spawnButton.RootGameObject.SetActive(true);
        _spawnIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"));
        _spawnButton.Label.SetText("Back");

        if (ModeManager.CurrentMode.GetType() == typeof(MatchMode) &&
            DynamicUIManager.PanelExists<SpawnLocationPanel>()) {
            ConfigRobot = MatchMode.Robots[DynamicUIManager.GetPanel<SpawnLocationPanel>().SelectedButton];
        } else {
            ConfigRobot = RobotSimObject.GetCurrentlyPossessedRobot();
        }

        RemoveAllItemsFromDrawer();
        
        AddItemToDrawer("Pickup", b => {
            if (DynamicUIManager.PanelExists<ConfigureShotTrajectoryPanel>())
                DynamicUIManager.ClosePanel<ConfigureShotTrajectoryPanel>();
            DynamicUIManager.CreatePanel<ConfigureGamepiecePickupPanel>();
        }, drawerPosition: DrawerPosition.Top);
        AddItemToDrawer("Ejector", b => {
            if (DynamicUIManager.PanelExists<ConfigureGamepiecePickupPanel>())
                DynamicUIManager.ClosePanel<ConfigureGamepiecePickupPanel>();
            DynamicUIManager.CreatePanel<ConfigureShotTrajectoryPanel>();
        }, drawerPosition: DrawerPosition.Top);

        AddItemToDrawer("RoboRIO", b => DynamicUIManager.CreateModal<RioConfigurationModal>(true),
            drawerPosition: DrawerPosition.Bottom, icon: SynthesisAssetCollection.GetSpriteByName("roborio"));
        AddItemToDrawer("Drivetrain", b => DynamicUIManager.CreateModal<ChangeDrivetrainModal>(),
            drawerPosition: DrawerPosition.Bottom, icon: SynthesisAssetCollection.GetSpriteByName("drivetrain"));
        AddItemToDrawer("Motors", b => { DynamicUIManager.CreateModal<ConfigMotorModal>(); },
            drawerPosition: DrawerPosition.Bottom);

        if (ModeManager.CurrentMode.GetType() == typeof(PracticeMode))
            AddItemToDrawer("Move", b => {
                if (!isMatchFreeCam)
                    OrbitCameraMode.FocusPoint = () =>
                        ConfigRobot.GroundedNode != null && ConfigRobot.GroundedBounds != null
                            ? ConfigRobot.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(
                                  ConfigRobot.GroundedBounds.center)
                            : Vector3.zero;
                GizmoManager.SpawnGizmo(ConfigRobot);
            }, drawerPosition: DrawerPosition.Bottom);

        if (MatchStateMachine.Instance.CurrentState.StateName is MatchStateMachine.StateName.RobotPositioning) {
            isMatchFreeCam =
                Camera.main.GetComponent<CameraController>().CameraMode == CameraController.CameraModes["Freecam"];
        } else {
            isMatchFreeCam          = false;
            PhysicsManager.IsFrozen = true;
        }
    }

    public static void LeaveConfig() {
        DynamicUIManager.CloseAllPanels();
        GizmoManager.ExitGizmo();
        isConfig = false;
        if (ModeManager.CurrentMode.GetType() == typeof(PracticeMode)) {
            SetUpPractice();
        } else if (ModeManager.CurrentMode.GetType() == typeof(MatchMode)) {
            SetUpMatch();
            if (PhysicsManager.IsFrozen &&
                !(MatchStateMachine.Instance.CurrentState.StateName is MatchStateMachine.StateName.RobotPositioning))
                PhysicsManager.IsFrozen = false;
        }
    }
}
