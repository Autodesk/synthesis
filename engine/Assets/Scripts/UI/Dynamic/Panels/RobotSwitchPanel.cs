using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

public class RobotSwitchPanel : PanelDynamic {
    private const float PANEL_WIDTH = 400f;

    private ScrollView _scrollView;
    private Button _addButton;
    private Button _removeButton;

    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin);
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f, rightPadding: 15f); // used to be 15f
        return u;
    };

    public Func<Button, Button> EnableButton = b =>
        b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
            .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT))
            .EnableEvents<Button>();

    public Func<Button, Button> DisableButton = b =>
        b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
            .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_ORANGE_CONTRAST_TEXT))
            .DisableEvents<Button>();

    public RobotSwitchPanel() : base(new Vector2(PANEL_WIDTH, 400)) {}

    // Update is called once per frame
    public override bool Create() {
        Title.SetText("Robot Switcher");
        CancelButton.RootGameObject.SetActive(false);
        AcceptButton.StepIntoLabel(l => l.SetText("Close"));
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<RobotSwitchPanel>());
        _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

        (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50))
                                            .SetBottomStretch<Content>()
                                            .SplitLeftRight((PANEL_WIDTH - 10f) / 2f, 10f);

        _addButton = left.CreateButton("Add").SetStretch<Button>().AddOnClickedEvent(
            b => { DynamicUIManager.CreateModal<AddRobotModal>(); });
        _removeButton = right.CreateButton("Remove").SetStretch<Button>().AddOnClickedEvent(b => {
            RobotSimObject.RemoveRobot(RobotSimObject.CurrentlyPossessedRobot);
            PopulateScrollView();
            if (RobotSimObject.SpawnedRobots.Count < RobotSimObject.MAX_ROBOTS)
                _addButton.ApplyTemplate<Button>(EnableButton);
        });

        PopulateScrollView();

        if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
            _removeButton.ApplyTemplate(DisableButton);

        if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS)
            _addButton.ApplyTemplate(DisableButton);

        EventBus.NewTypeListener<RobotSimObject.PossessionChangeEvent>(PossessedRobotChanged);
        EventBus.NewTypeListener<RobotSimObject.RobotSpawnEvent>(RobotSpawned);
        EventBus.NewTypeListener<RobotSimObject.RobotRemoveEvent>(RobotRemoved);

        return true;
    }

    private void AddEntry(RobotSimObject robot) {
        var toggle = _scrollView.Content
                         .CreateToggle(isOn: RobotSimObject.CurrentlyPossessedRobot == robot.Name, label: robot.Name)
                         .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f))
                         .ApplyTemplate(VerticalLayout)
                         .StepIntoLabel(l => l.SetFontSize(16f))
                         .SetDisabledColor(ColorManager.SYNTHESIS_BLACK);
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
            _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => {
                x.DisableEvents<Toggle>();
                x.State = false;
                x.EnableEvents<Toggle>();
            });
            toggle.DisableEvents<Toggle>();
            toggle.State = true;
            toggle.EnableEvents<Toggle>();
        } else {
            RobotSimObject.CurrentlyPossessedRobot = string.Empty;
        }
    }

    private void PossessedRobotChanged(IEvent e) {
        var possChangeEvent = e as RobotSimObject.PossessionChangeEvent;

        if (possChangeEvent == null)
            return;

        if (possChangeEvent.NewBot == string.Empty)
            _removeButton.ApplyTemplate(DisableButton);
        else
            _removeButton.ApplyTemplate(EnableButton);
    }

    private void RobotSpawned(IEvent e) {
        var newRobotEvent = e as RobotSimObject.RobotSpawnEvent;
        if (newRobotEvent == null)
            return;

        PopulateScrollView();
    }

    private void RobotRemoved(IEvent e) {
        var newRobotEvent = e as RobotSimObject.RobotRemoveEvent;
        if (newRobotEvent == null)
            return;

        PopulateScrollView();
    }

    public override void Update() {}

    public override void Delete() {
        EventBus.RemoveTypeListener<RobotSimObject.PossessionChangeEvent>(PossessedRobotChanged);
        EventBus.RemoveTypeListener<RobotSimObject.RobotSpawnEvent>(RobotSpawned);
        EventBus.RemoveTypeListener<RobotSimObject.RobotRemoveEvent>(RobotRemoved);
    }
}
