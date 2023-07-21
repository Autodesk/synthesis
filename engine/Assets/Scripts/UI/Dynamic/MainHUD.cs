using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.Dynamic;

using DigitalRuby.Tween;
using SynthesisAPI.EventBus;
using UnityEngine.SceneManagement;
using Synthesis.Runtime;
using Synthesis.Physics;
using SynthesisAPI.Utilities;
using Synthesis.Gizmo;
using Modes.MatchMode;
using Utilities.ColorManager;

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

    public static bool isConfig       = false;
    public static bool isMatchFreeCam = false;

    public static RobotSimObject ConfigRobot = RobotSimObject.GetCurrentlyPossessedRobot();

    private static Content _tabDrawerContent;
    private static Button _expandDrawerButton;
    private static Image _expandIcon;

    private static List<(Button button, Image image)> _drawerItems = new List<(Button button, Image image)>();

    public static List<string> DrawerTitles = new List<string>();

    public static void Setup() {
        _drawerItems.Clear();
        _tabDrawerContent = new Content(null, GameObject.Find("MainHUD").transform.Find("TabDrawer").gameObject, null);
        _expandDrawerButton = new Button(
            _tabDrawerContent, _tabDrawerContent.RootGameObject.transform.Find("ExpandButton").gameObject, null);

        _expandDrawerButton.AddOnClickedEvent(b => MainHUD.Collapsed = !MainHUD.Collapsed);
        _expandIcon = new Image(null, _expandDrawerButton.RootGameObject.transform.Find("Icon").gameObject);

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
                    MainHUD.AddItemToDrawer("Configure", b => SetUpConfig(), index: 0,
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
        if (!SimulationRunner.InSim)
            return;

        var drawerButtonObj = GameObject.Instantiate(SynthesisAssetCollection.GetUIPrefab("hud-drawer-item-base"),
            _tabDrawerContent.RootGameObject.transform.Find("ItemContainer"));
        var drawerButton    = new Button(_tabDrawerContent, drawerButtonObj, null);
        drawerButton.Label!.SetText(title);
        drawerButton.AddOnClickedEvent(onClick);
        var drawerIcon = new Image(_tabDrawerContent, drawerButtonObj.transform.Find("ItemIcon").gameObject);
        if (icon != null)
            drawerIcon.SetSprite(icon);

        if (index < 0 || index > _drawerItems.Count) {
            _drawerItems.Add((drawerButton, drawerIcon));
        } else {
            _drawerItems.Insert(index, (drawerButton, drawerIcon));
        }

        DrawerTitles.Add(title);

        UpdateDrawerSizing();
        AssignColors(null);
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

    public static void Delete() {
        EventBus.RemoveTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public static void AssignColors(IEvent e) {
        _tabDrawerContent.Image!.SetColor(ColorManager.SynthesisColor.Background);

        _expandDrawerButton.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.Background));
        _expandIcon.SetColor(ColorManager.SynthesisColor.Icon);

        _drawerItems.ForEach(x => {
            x.button.Label!.SetColor(ColorManager.SynthesisColor.MainText);
            x.button.Image.SetColor(ColorManager.SynthesisColor.Background);
            x.image.SetColor(ColorManager.SynthesisColor.InteractiveElementSolid);
        });
    }

    public static void SetUpPractice() {
        foreach (string name in MainHUD.DrawerTitles)
            MainHUD.RemoveItemFromDrawer(name);

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            MainHUD.AddItemToDrawer(
                "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        MainHUD.AddItemToDrawer("Spawn", b => DynamicUIManager.CreateModal<SpawningModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("PlusIcon"));
        MainHUD.AddItemToDrawer("Multibot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>());
        MainHUD.AddItemToDrawer("Scoring Zones", b => {
            if (FieldSimObject.CurrentField == null) {
                SynthesisAPI.Utilities.Logger.Log("No field loaded!", LogLevel.Info);
            } else {
                if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                    DynamicUIManager.CreatePanel<ScoringZonesPanel>();
            }
        });
        MainHUD.AddItemToDrawer("Camera View", b => DynamicUIManager.CreateModal<ChangeViewModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("CameraIcon"));
        MainHUD.AddItemToDrawer("Download Asset", b => DynamicUIManager.CreateModal<DownloadAssetModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("DownloadIcon"));
        MainHUD.AddItemToDrawer("DriverStation",
            b => DynamicUIManager.CreatePanel<BetaWarningPanel>(
                false, (Action) (() => DynamicUIManager.CreatePanel<DriverStationPanel>(true))),
            icon: SynthesisAssetCollection.GetSpriteByName("driverstation-icon"));
        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));

        PhysicsManager.IsFrozen = false;
    }

    public static void SetUpMatch() {
        foreach (string name in MainHUD.DrawerTitles)
            MainHUD.RemoveItemFromDrawer(name);

        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            MainHUD.AddItemToDrawer(
                "Configure", b => SetUpConfig(), icon: SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        MainHUD.AddItemToDrawer("Multibot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>());
        MainHUD.AddItemToDrawer("Camera View", b => DynamicUIManager.CreateModal<ChangeViewModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("CameraIcon"));
        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));
        MainHUD.AddItemToDrawer("DriverStation",
            b => DynamicUIManager.CreatePanel<BetaWarningPanel>(
                false, (Action) (() => DynamicUIManager.CreatePanel<DriverStationPanel>(true))),
            icon: SynthesisAssetCollection.GetSpriteByName("driverstation-icon"));
        MainHUD.AddItemToDrawer("Scoring Zones", b => {
            if (FieldSimObject.CurrentField == null) {
                SynthesisAPI.Utilities.Logger.Log("No field loaded!", LogLevel.Info);
            } else {
                if (!DynamicUIManager.PanelExists<ScoringZonesPanel>())
                    DynamicUIManager.CreatePanel<ScoringZonesPanel>();
            }
        });

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

        if (ModeManager.CurrentMode.GetType() == typeof(MatchMode) &&
            DynamicUIManager.PanelExists<SpawnLocationPanel>()) {
            ConfigRobot = MatchMode.Robots[DynamicUIManager.GetPanel<SpawnLocationPanel>().SelectedButton];
        } else {
            ConfigRobot = RobotSimObject.GetCurrentlyPossessedRobot();
        }

        foreach (string name in MainHUD.DrawerTitles)
            MainHUD.RemoveItemFromDrawer(name);

        MainHUD.AddItemToDrawer("Back", b => { LeaveConfig(); });

        MainHUD.AddItemToDrawer("Pickup", b => {
            if (DynamicUIManager.PanelExists<ConfigureShotTrajectoryPanel>())
                DynamicUIManager.ClosePanel<ConfigureShotTrajectoryPanel>();
            DynamicUIManager.CreatePanel<ConfigureGamepiecePickupPanel>();
        });
        MainHUD.AddItemToDrawer("Ejector", b => {
            if (DynamicUIManager.PanelExists<ConfigureGamepiecePickupPanel>())
                DynamicUIManager.ClosePanel<ConfigureGamepiecePickupPanel>();
            DynamicUIManager.CreatePanel<ConfigureShotTrajectoryPanel>();
        });

        MainHUD.AddItemToDrawer("Motors", b => { DynamicUIManager.CreateModal<ConfigMotorModal>(); });
        MainHUD.AddItemToDrawer("Controls", b => DynamicUIManager.CreateModal<ChangeInputsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("DriverStationView"));
        MainHUD.AddItemToDrawer("RoboRIO Conf.", b => DynamicUIManager.CreateModal<RioConfigurationModal>(true),
            icon: SynthesisAssetCollection.GetSpriteByName("rio-config-icon"));
        MainHUD.AddItemToDrawer("Drivetrain", b => DynamicUIManager.CreateModal<ChangeDrivetrainModal>());
        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));

        if (ModeManager.CurrentMode.GetType() == typeof(PracticeMode))
            MainHUD.AddItemToDrawer("Move", b => {
                if (!isMatchFreeCam)
                    OrbitCameraMode.FocusPoint = () =>
                        ConfigRobot.GroundedNode != null && ConfigRobot.GroundedBounds != null
                            ? ConfigRobot.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(
                                  ConfigRobot.GroundedBounds.center)
                            : Vector3.zero;
                GizmoManager.SpawnGizmo(ConfigRobot);
            });

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
