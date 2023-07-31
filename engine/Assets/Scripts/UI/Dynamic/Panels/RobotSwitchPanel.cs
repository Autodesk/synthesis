using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modes.MatchMode;
using SimObjects.MixAndMatch;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UI.Dynamic.Modals.Spawning;
using UI.Dynamic.Panels.Tooltip;
using UnityEngine;
using Utilities.ColorManager;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

public class RobotSwitchPanel : PanelDynamic {
    private const float PANEL_WIDTH = 300f;

    private ScrollView _scrollView;
    private Button _addButton;
    private Button _removeButton;

    private bool _isMatchMode;

    public RobotSwitchPanel() : base(new Vector2(PANEL_WIDTH, 400)) {
        _isMatchMode = ModeManager.CurrentMode.GetType() == typeof(MatchMode);
    }

    // Update is called once per frame
    public override bool Create() {
        Title.SetText("Robot Switcher");
        CancelButton.RootGameObject.SetActive(false);
        AcceptButton.StepIntoLabel(l => l.SetText("Close"));
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<RobotSwitchPanel>());
        _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

        (Content left, Content right) = MainContent.CreateSubContent(new Vector2(PANEL_WIDTH, 50))
                                            .SetBottomStretch<Content>()
                                            .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

        if (!_isMatchMode) {
            _addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
                b => { DynamicUIManager.CreateModal<AddRobotModal>(); });
            _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(b => {
                RobotSimObject.RemoveRobot(RobotSimObject.CurrentlyPossessedRobot);
                PopulateScrollView();
                if (RobotSimObject.SpawnedRobots.Count < RobotSimObject.MAX_ROBOTS)
                    _addButton.ApplyTemplate<Button>(Button.EnableButton);
            });

            if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
                _removeButton.ApplyTemplate(Button.DisableButton);

            if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS)
                _addButton.ApplyTemplate(Button.DisableButton);
        }

        PopulateScrollView();

        EventBus.NewTypeListener<RobotSimObject.PossessionChangeEvent>(PossessedRobotChanged);
        EventBus.NewTypeListener<RobotSimObject.RobotSpawnEvent>(RobotSpawned);
        EventBus.NewTypeListener<RobotSimObject.RobotRemoveEvent>(RobotRemoved);

        return true;
    }

    private void AddEntry(RobotSimObject robot) {
        var toggle =
            _scrollView.Content.CreateToggle(true, RobotSimObject.CurrentlyPossessedRobot == robot.Name, robot.Name)
                .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 40f))
                .ApplyTemplate(Toggle.RadioToggleLayout)
                .StepIntoLabel(l => l.SetFontSize(16f))
                .SetDisabledColor(ColorManager.SynthesisColor.Background);
        toggle.AddOnStateChangedEvent((t, s) => { UpdateState(robot, t, s); });
    }

    private void PopulateScrollView() {
        _scrollView.Content.DeleteAllChildren();
        RobotSimObject.SpawnedRobots.ForEach(AddEntry);
        _scrollView.Content.SetHeight<Content>(_scrollView.Content.HeightOfChildren);
    }

    private void UpdateState(RobotSimObject robot, Toggle toggle, bool state) {
        if (state) {
            RobotSimObject.CurrentlyPossessedRobot = robot.Name;
            MainHUD.ConfigRobot                    = robot;
            _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => { x.SetStateWithoutEvents(false); });
            toggle.SetStateWithoutEvents(true);
        } else {
            MainHUD.ConfigRobot                    = null;
            RobotSimObject.CurrentlyPossessedRobot = string.Empty;
        }
    }

    private void PossessedRobotChanged(IEvent e) {
        var possChangeEvent = e as RobotSimObject.PossessionChangeEvent;

        if (possChangeEvent == null)
            return;

        if (!_isMatchMode) {
            if (possChangeEvent.NewBot == string.Empty)
                _removeButton.ApplyTemplate(Button.EnableButton);
            else
                _removeButton.ApplyTemplate(Button.DisableButton);
        }
    }

    private void RobotSpawned(IEvent e) {
        var newRobotEvent = e as RobotSimObject.RobotSpawnEvent;
        if (newRobotEvent == null)
            return;

        PopulateScrollView();

        if (!_isMatchMode) {
            if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
                _removeButton.ApplyTemplate(Button.DisableButton);

            if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS)
                _addButton.ApplyTemplate(Button.DisableButton);
        }
    }

    private void RobotRemoved(IEvent e) {
        var newRobotEvent = e as RobotSimObject.RobotRemoveEvent;
        if (newRobotEvent == null)
            return;

        PopulateScrollView();

        if (!_isMatchMode) {
            if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
                _removeButton.ApplyTemplate(Button.EnableButton);

            if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS)
                _addButton.ApplyTemplate(Button.DisableButton);
        }
    }

    public override void Update() {}

    public override void Delete() {
        EventBus.RemoveTypeListener<RobotSimObject.PossessionChangeEvent>(PossessedRobotChanged);
        EventBus.RemoveTypeListener<RobotSimObject.RobotSpawnEvent>(RobotSpawned);
        EventBus.RemoveTypeListener<RobotSimObject.RobotRemoveEvent>(RobotRemoved);
    }
}
