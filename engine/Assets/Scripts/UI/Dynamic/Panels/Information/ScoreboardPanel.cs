using System;
using Modes.MatchMode;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;

public class ScoreboardPanel : PanelDynamic {
    private const float PANEL_HEIGHT_WITHOUT_TIMER = 100;
    private const float PANEL_HEIGHT_WITH_TIMER    = 150;

    private const float VERTICAL_PADDING = 10f;
    private const float INSET_PADDING    = 10f;

    private const float PANEL_WIDTH = 200f;
    private float PANEL_HEIGHT;

    private readonly bool _showTimer;

    private Label time, redScore, blueScore;
    private Content topContent, bottomContent;

    public Func<UIComponent, UIComponent> VerticalLayout = u => {
        float offset = -u.Parent!.RectOfChildren(u).yMin + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };

    public ScoreboardPanel(bool showTimer = true)
        : base(new Vector2(PANEL_WIDTH, showTimer ? PANEL_HEIGHT_WITH_TIMER : PANEL_HEIGHT_WITHOUT_TIMER)) {
        _showTimer   = showTimer;
        PANEL_HEIGHT = showTimer ? PANEL_HEIGHT_WITH_TIMER : PANEL_HEIGHT_WITHOUT_TIMER;
    }

    public override bool Create() {
        TweenDirection = Vector2.down;

        var newMainContent = Strip(new Vector2(PANEL_WIDTH, PANEL_HEIGHT + (_showTimer ? VERTICAL_PADDING : 0)),
            leftPadding: INSET_PADDING, rightPadding: INSET_PADDING, topPadding: INSET_PADDING,
            bottomPadding: INSET_PADDING);

        float bottomHeight = PANEL_HEIGHT;

        if (_showTimer) {
            float topHeight = 50f;
            bottomHeight -= topHeight;
            (topContent, bottomContent) = newMainContent.SplitTopBottom(topHeight, VERTICAL_PADDING);

            bottomContent.SetHeight<Content>(bottomHeight);

            time = topContent.CreateLabel(topHeight)
                       .SetStretch<Label>()
                       .SetText(MatchMode.MatchTime.ToString())
                       .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                       .SetVerticalAlignment(VerticalAlignmentOptions.Middle)
                       .SetFontSize(40);
        } else {
            bottomContent = newMainContent;
        }

        float leftRightPadding              = 8;
        float leftWidth                     = (bottomContent.Size.x - leftRightPadding) / 2;
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
        blueScore = rightContent.CreateLabel()
                        .ApplyTemplate(VerticalLayout)
                        .SetText("0")
                        .SetFontSize(scoreSize)
                        .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
                        .SetAnchoredPosition<Label>(new Vector2(0, -bottomHeight / 2))
                        .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                        .SetVerticalAlignment(VerticalAlignmentOptions.Middle);

        return true;
    }

    public override void Update() {
        if (SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT) ||
            SimulationRunner.HasContext(SimulationRunner.PAUSED_SIM_CONTEXT))
            return;

        if (_showTimer) {
            // state advances in MatchMode update
            if (MatchStateMachine.Instance.CurrentState.StateName is >= MatchStateMachine.StateName.Auto and <=
                MatchStateMachine.StateName.Endgame and not MatchStateMachine.StateName.Transition) {
                MatchMode.MatchTime -= Time.deltaTime;
                time.SetText(Mathf.RoundToInt(MatchMode.MatchTime).ToString());
            }
        }

        redScore.SetText(Scoring.redScore.ToString());
        blueScore.SetText(Scoring.blueScore.ToString());
    }

    public override void Delete() {}
}
