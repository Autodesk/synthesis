using System;
using Synthesis.Runtime;
using TMPro;
using UnityEngine;
using Synthesis.UI.Dynamic;

public class ScoreboardPanel : PanelDynamic {
    private static float _width = 200f;
    private static float _height = 80f;
    
    public ScoreboardPanel() : base(new Vector2(_width, _height)) { }

    private Label time, redScore, blueScore;

    private const float VERTICAL_PADDING = 10f;
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };

    public override bool Create() {

        CancelButton.RootGameObject.SetActive(false);
        AcceptButton.RootGameObject.SetActive(false);
        Title.RootGameObject.SetActive(false);
        PanelImage.RootGameObject.SetActive(false);

        Content panel = new Content(null, UnityObject, null);
        // MainContent.SetTopStretch<Content>(Screen.width / 2 - _width / 2 - 40f, Screen.width / 2 - _width / 2 - 40f, 0);
        panel.SetBottomStretch<Content>(Screen.width / 2 - _width / 2, Screen.width / 2 - _width / 2, 0);
        
        time = MainContent.CreateLabel(15f).ApplyTemplate(VerticalLayout).SetTopStretch(leftPadding: 0f, anchoredY: -30f).
            SetText(targetTime.ToString()).SetHorizontalAlignment(HorizontalAlignmentOptions.Center).SetFontSize(40);

        float leftRightPadding = 8;
        float leftWidth = (MainContent.Size.x - leftRightPadding) / 2;
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(leftWidth, leftRightPadding);
        

        leftContent.SetBackgroundColor<Content>(Color.red).CreateLabel(50f).ApplyTemplate(VerticalLayout).SetTopStretch(leftPadding: 0f, anchoredY: 10f).SetText("RED")
            .SetFontSize(30).SetHorizontalAlignment(HorizontalAlignmentOptions.Center);
        redScore = leftContent.CreateLabel(50f).ApplyTemplate(VerticalLayout).SetTopStretch(leftPadding: 10f, anchoredY: 60f).SetText("0")
            .SetFontSize(50).SetHorizontalAlignment(HorizontalAlignmentOptions.Center);
        rightContent.SetBackgroundColor<Content>(Color.blue).CreateLabel(50f).ApplyTemplate(VerticalLayout).SetTopStretch(leftPadding: 0f, anchoredY: 10f).SetText("BLUE")
            .SetFontSize(30).SetHorizontalAlignment(HorizontalAlignmentOptions.Center);
        blueScore = rightContent.CreateLabel(50f).ApplyTemplate(VerticalLayout).SetTopStretch(leftPadding: 0f, anchoredY: 60f).SetText("0")
            .SetFontSize(50).SetHorizontalAlignment(HorizontalAlignmentOptions.Center);

        return true;
    }
    
    float targetTime = 135;
    bool matchEnd = false;
    public override void Update() {
        if (!(SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT) || SimulationRunner.HasContext(SimulationRunner.PAUSED_SIM_CONTEXT)) 
                && targetTime >= 0) {
            targetTime -= Time.deltaTime;
            time.SetText(Mathf.RoundToInt(targetTime).ToString());
            redScore.SetText(Scoring.redScore.ToString());
            blueScore.SetText(Scoring.blueScore.ToString());
        }
        else if (!matchEnd)
        {
            //end match
            matchEnd = true;
        }
    }

    public override void Delete() { }
}