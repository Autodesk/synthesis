using System;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;
public class ScoreboardPanel : PanelDynamic {

    private const float VERTICAL_PADDING = 10f;
    private static readonly float WIDTH = 200f;
    private static float HEIGHT;

    private readonly bool _showTimer;

    private Label time, redScore, blueScore;
    private Content topContent, bottomContent;

    public Func<UIComponent, UIComponent> VerticalLayout = u => {
        float offset = -u.Parent!.RectOfChildren(u).yMin + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };

    public ScoreboardPanel(bool showTimer = true) : base(new Vector2(WIDTH, showTimer ? 150 : 100)) {
        _showTimer = showTimer;
        HEIGHT = showTimer ? 150f : 100f;
    }

    public override bool Create() {

        CancelButton.RootGameObject.SetActive(false);
        AcceptButton.RootGameObject.SetActive(false);
        Title.RootGameObject.SetActive(false);
        PanelImage.RootGameObject.SetActive(false);

        var panel = new Content(null, UnityObject, null);
        panel.SetBottomStretch<Content>(Screen.width / 2 - WIDTH / 2, Screen.width / 2 - WIDTH / 2);

        float bottomHeight = HEIGHT;

        if (_showTimer) {
            float topHeight = 50f;
            bottomHeight -= topHeight;
            (topContent, bottomContent) = MainContent.SplitTopBottom(topHeight, 0f);

            topContent.SetAnchoredPosition<Content>(new Vector2(0, -topHeight / 2));
            bottomContent.SetAnchoredPosition<Content>(new Vector2(0, -topHeight / 2));
            time = topContent.CreateLabel(topHeight)
                .SetStretch<Label>()
                .ApplyTemplate(VerticalLayout)
                .SetAnchors<Label>(new Vector2(0, 0.5f), new Vector2(1, 0.5f))
                .SetAnchoredPosition<Label>(new Vector2(0, topHeight / 2))
                .SetText(Scoring.targetTime.ToString())
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                .SetFontSize(40);
        } else {
            bottomContent = MainContent;
        }

        float leftRightPadding = 8;
        float leftWidth = (bottomContent.Size.x - leftRightPadding) / 2;
        (var leftContent, var rightContent) = bottomContent.SplitLeftRight(leftWidth, leftRightPadding);
        leftContent.SetBackgroundColor<Content>(Color.red);
        rightContent.SetBackgroundColor<Content>(Color.blue);

        const float titleSize = 20;
        const float scoreSize = 50;

        leftContent.CreateLabel()
            .ApplyTemplate(VerticalLayout)
            .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
            .SetAnchoredPosition<Label>(new Vector2(0, -10))
            .SetText("RED")
            .SetFontSize(titleSize)
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        redScore = leftContent.CreateLabel()
            .ApplyTemplate(VerticalLayout)
            .SetText("0")
            .SetFontSize(scoreSize)
            .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
            .SetAnchoredPosition<Label>(new Vector2(0, -bottomHeight / 2))
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        rightContent.CreateLabel()
            .ApplyTemplate(VerticalLayout)
            .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
            .SetAnchoredPosition<Label>(new Vector2(0, -10))
            .SetText("BLUE")
            .SetFontSize(titleSize)
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        blueScore = rightContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("0")
            .SetFontSize(scoreSize)
            .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
            .SetAnchoredPosition<Label>(new Vector2(0, -bottomHeight / 2))
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetVerticalAlignment(VerticalAlignmentOptions.Middle);

        return true;
    }

    public override void Update() {
        if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT) || SimulationRunner.HasContext(SimulationRunner.PAUSED_SIM_CONTEXT)) return;

        if (_showTimer) {
            // state advances in MatchMode update
            if (MatchStateMachine.Instance.CurrentState.StateName == MatchStateMachine.StateName.Auto ||
                MatchStateMachine.Instance.CurrentState.StateName == MatchStateMachine.StateName.Teleop) {
                Scoring.targetTime -= Time.deltaTime;
                time.SetText(Mathf.RoundToInt(Scoring.targetTime).ToString());
            }
        }

        redScore.SetText(Scoring.redScore.ToString());
        blueScore.SetText(Scoring.blueScore.ToString());
    }

    public override void Delete() { }
}
