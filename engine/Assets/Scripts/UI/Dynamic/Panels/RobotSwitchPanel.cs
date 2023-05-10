using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Utilities;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

public class RobotSwitchPanel : PanelDynamic {

    private const float PANEL_WIDTH = 400f;

    private ScrollView _scrollView;
    private Button _addButton;
    private Button _removeButton;
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) =>
    {
        var offset = (-u.Parent!.RectOfChildren(u).yMin);
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f, rightPadding: 15f); // used to be 15f
        return u;
    };

    public RobotSwitchPanel() : base(new Vector2(PANEL_WIDTH, 400)) { }

    // Update is called once per frame
    public override bool Create() {

        if (RobotSimObject.SpawnedRobots.Count == 0) {
            Logger.Log("No robots spawned", LogLevel.Info);
            return false;
        }

        Title.SetText("Robot Switcher");
        CancelButton.RootGameObject.SetActive(false);
        AcceptButton.StepIntoLabel(l => l.SetText("Close"));
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<RobotSwitchPanel>());
        _scrollView = MainContent.CreateScrollView().SetStretch<ScrollView>(bottomPadding: 60f);

        (Content left, Content right) = MainContent.CreateSubContent(new Vector2(400, 50)).SetBottomStretch<Content>()
            .SplitLeftRight((PANEL_WIDTH + 10f) / 2f - 10f, 10f);

        _addButton = left.CreateButton("Add").SetStretch<Button>();
        _removeButton = right.CreateButton("Remove").SetStretch<Button>();
        
        PopulateScrollView();
        
        return true;
    }

    private void AddEntry(RobotSimObject robot) {
        var toggle = _scrollView.Content.CreateToggle(isOn: RobotSimObject.CurrentlyPossessedRobot == robot.Name, label: robot.Name)
            .SetSize<Toggle>(new Vector2(PANEL_WIDTH, 50f)).ApplyTemplate(VerticalLayout)
            .StepIntoLabel(l => l.SetFontSize(16f))
            .SetDisabledColor(ColorManager.SYNTHESIS_BLACK);
        toggle.AddOnStateChangedEvent((t, s) => {
            UpdateState(robot, t, s);
        });
    }

    private void PopulateScrollView() {
        _scrollView.Content.DeleteAllChildren();
        RobotSimObject.SpawnedRobots.ForEach(x => AddEntry(x));
        _scrollView.Content.SetHeight<Content>(_scrollView.Content.HeightOfChildren);
    }

    private void UpdateState(RobotSimObject robot, Toggle toggle, bool state) {
        if (state) {
            robot.Possess();
            _scrollView.Content.ChildrenReadOnly.OfType<Toggle>().ForEach(x => {
                x.DisableEvents<Toggle>();
                x.State = false;
                x.EnableEvents<Toggle>();
            });
            toggle.DisableEvents<Toggle>();
            toggle.State = true;
            toggle.EnableEvents<Toggle>();
        } else {
            robot.Unpossess();
        }
    }
    
    public override void Update() { }
    public override void Delete() { }
    public RobotSwitchPanel(Vector2 mainContentSize, float leftContentPadding = 20, float rightContentPadding = 20): base(mainContentSize, leftContentPadding, rightContentPadding) { }
}
