using Synthesis.UI.Dynamic;
using UnityEngine;
using System;
using SynthesisAPI.Simulation;

public class ConfigMotorModal : ModalDynamic {
    const float MODAL_HEIGHT = 500f;
    const float MODAL_WIDTH = 600f;
    const float PADDING = 16f;
    const float MOTOR_HEIGHT = 40f;
    const float NAME_WIDTH = 180f;
    const float SCROLL_WIDTH = 10f;

    private ScrollView _scrollView;
    private float _scrollViewWidth;

    private Slider velSlider;

    public ConfigMotorModal(): base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: PADDING, rightPadding: PADDING);
        return u;
    };

    // private readonly Func<UIComponent, UIComponent> 

    public override void Create() {
        Title.SetText("Motor Configuration").SetWidth<Label>(MODAL_WIDTH - PADDING * 4);
        Description.SetText("Change motor settings");

        AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
            RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.Save();
            DynamicUIManager.CloseActiveModal();
        });

        (Content nameLabelContent, Content velLabelContent) = MainContent.CreateSubContent(new Vector2(MODAL_WIDTH - SCROLL_WIDTH, 20f))
            .SetTopStretch<Content>(PADDING, PADDING, 0)
            .SplitLeftRight(NAME_WIDTH, PADDING);

        nameLabelContent.CreateLabel().SetText("Motor");//.SetTopStretch(PADDING, PADDING, 0);
        velLabelContent.CreateLabel().SetText("Target Velocity"); //.SetLeftStretch(PADDING, PADDING, 0).SetTopStretch(PADDING, PADDING, 0);
        
        _scrollView = MainContent.CreateScrollView().SetRightStretch<ScrollView>()
            .SetTopStretch<ScrollView>(0,0, -nameLabelContent.Parent!.RectOfChildren().yMin + PADDING * 2)
            .SetHeight<ScrollView>(MODAL_HEIGHT - nameLabelContent.Parent!.RectOfChildren().yMin - 50);

        _scrollViewWidth = _scrollView.Parent!.RectOfChildren().width - SCROLL_WIDTH;
    
        (Content nameContent, Content velContent) = _scrollView.Content
            .CreateSubContent(new Vector2(_scrollViewWidth, MOTOR_HEIGHT))
            .SetTopStretch<Content>(0,0,0)
            .ApplyTemplate<Content>(VerticalLayout)
            .SplitLeftRight(NAME_WIDTH, PADDING);
        nameContent.CreateLabel().SetText("Drivetrain")
            .SetTopStretch(0, PADDING, PADDING + _scrollView.HeightOfChildren);
        velSlider = velContent.CreateSlider("", minValue: 0f, maxValue: 200f, currentValue: RobotSimObject.GetCurrentlyPossessedRobot().GetLeftRightWheels()!.Value.leftWheels[0].Motor.targetVelocity)
            .SetTopStretch<Slider>(PADDING, PADDING, _scrollView.HeightOfChildren);
        
    }

    public override void Update() { }
    public override void Delete() { }

}