using System;
using Modes.MatchMode;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

public class ScoreboardPanel : PanelDynamic {
    private const float PANEL_HEIGHT_WITHOUT_TIMER = 100;
    private const float PANEL_HEIGHT_WITH_TIMER    = 150;

    private const float VERTICAL_PADDING = 10f;
    private const float INSET_PADDING    = 10f;

    private const float PANEL_WIDTH = 200f;
    private float PANEL_HEIGHT;

    private readonly bool _showTimer;

    private Label _time, _redScore, _blueScore, _redLabel, _blueLabel;
    private Content _topContent, _bottomContent;

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
            (_topContent, _bottomContent) = newMainContent.SplitTopBottom(topHeight, VERTICAL_PADDING);

            _bottomContent.SetHeight<Content>(bottomHeight);

            _time = _topContent.CreateLabel(topHeight)
                        .SetStretch<Label>()
                        .SetText(MatchMode.MatchTime.ToString())
                        .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                        .SetVerticalAlignment(VerticalAlignmentOptions.Middle)
                        .SetFontSize(40);
        } else {
            _bottomContent = newMainContent;
        }

        float leftRightPadding              = 8;
        float leftWidth                     = (_bottomContent.Size.x - leftRightPadding) / 2;
        (var leftContent, var rightContent) = _bottomContent.SplitLeftRight(leftWidth, leftRightPadding);
        leftContent.SetBackgroundColor<Content>(Color.red);
        rightContent.SetBackgroundColor<Content>(Color.blue);

        const float titleSize = 20;
        const float scoreSize = 50;

        _redLabel = leftContent.CreateLabel()
                        .ApplyTemplate(VerticalLayout)
                        .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
                        .SetAnchoredPosition<Label>(new Vector2(0, -10))
                        .SetText("RED")
                        .SetFontSize(titleSize)
                        .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                        .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        _redScore = leftContent.CreateLabel()
                        .ApplyTemplate(VerticalLayout)
                        .SetText("0")
                        .SetFontSize(scoreSize)
                        .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
                        .SetAnchoredPosition<Label>(new Vector2(0, -bottomHeight / 2))
                        .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                        .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        _blueLabel = rightContent.CreateLabel()
                         .ApplyTemplate(VerticalLayout)
                         .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
                         .SetAnchoredPosition<Label>(new Vector2(0, -10))
                         .SetText("BLUE")
                         .SetFontSize(titleSize)
                         .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                         .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        _blueScore = rightContent.CreateLabel()
                         .ApplyTemplate(VerticalLayout)
                         .SetText("0")
                         .SetFontSize(scoreSize)
                         .SetAnchors<Label>(new Vector2(0, 1), new Vector2(1, 1))
                         .SetAnchoredPosition<Label>(new Vector2(0, -bottomHeight / 2))
                         .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                         .SetVerticalAlignment(VerticalAlignmentOptions.Middle);
        AssignColors(null);
        EventBus.NewTypeListener<ColorManager.OnThemeChanged>(AssignColors);

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
                _time.SetText(Mathf.RoundToInt(MatchMode.MatchTime).ToString());
            }
        }

        _redScore.SetText(Scoring.redScore.ToString());
        _blueScore.SetText(Scoring.blueScore.ToString());
    }

    public override void Delete() {
        EventBus.RemoveTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public void AssignColors(IEvent e) {
        _redLabel.SetColor(ColorManager.SynthesisColor.MainText);
        _blueLabel.SetColor(ColorManager.SynthesisColor.MainText);
        _blueScore.SetColor(ColorManager.SynthesisColor.MainText);
        _redScore.SetColor(ColorManager.SynthesisColor.MainText);
    }
}
