using System;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using SynthesisAPI.Utilities;
using UnityEngine;
using Utilities.ColorManager;

#nullable enable

public class NetworkWaitModal : ModalDynamic {

    private const string PROGRESS_BAR_TWEEN = "network_progress_bar_tween";
    private const float PROGRESS_BAR_TWEEN_DURATION = 0.1f;
    
    private const float MODAL_WIDTH = 400;
    private const float MODAL_HEIGHT = 250;
    private const float EDGE_PADDING = 10f;
    private const float UPDATE_INTERVAL_SEC = 0.25f;

    private readonly NetworkTask<bool> _networkTask;

    private Label? _statusLabel;
    private Content? _progressContent;
    private readonly Action? _callback;

    public NetworkWaitModal(NetworkTask<bool> networkTask, Action? callback)
        : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _networkTask = networkTask;
        _callback = callback;
    }
    
    public override void Create() {
        
        Debug.Log("Wait Modal Created");
        
        var newContent = Strip(newContentSize: new Vector2(MODAL_WIDTH, MODAL_HEIGHT), leftPadding: EDGE_PADDING,
            rightPadding: EDGE_PADDING, topPadding: EDGE_PADDING, bottomPadding: EDGE_PADDING);
        _statusLabel = newContent.CreateLabel().SetStretch(bottomPadding: 10f).SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
            .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center).SetFontSize(20f);

        var container = newContent.CreateSubContent(new Vector2(20f, 10f)).SetBottomStretch<Content>(10f, 10f, 10f)
            .EnsureImage().StepIntoImage(i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.BackgroundSecondary).SetCornerRadius(5f));
		_progressContent = container.CreateSubContent(new Vector2(20f, 10f)).SetStretch<Content>().EnsureImage().StepIntoImage(
			i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.InteractiveElementLeft, ColorManager.SynthesisColor.InteractiveElementRight)
                .SetCornerRadius(5f));

        ExternalContent.SetPivot<Content>(Vector2.one / 2f).SetAnchors<Content>(Vector2.one / 2f, Vector2.one / 2f)
            .SetAnchoredPosition<Content>(Vector2.zero);
        
        UpdateStatusLabel();
	}

    private void UpdateStatusLabel() {
        var currentStatus = _networkTask.Status;
        
        SynthesisTween.CancelTween(PROGRESS_BAR_TWEEN);
        SynthesisTween.MakeTween(
            PROGRESS_BAR_TWEEN,
            _progressContent?.RootRectTransform.offsetMax.x,
            currentStatus.Progress,
            0.1f,
            (a, b, c) => SynthesisTweenInterpolationFunctions.FloatInterp(a, (float)b, (float)c),
            SynthesisTweenScaleFunctions.EaseOutCubic,
            x => _progressContent?.SetAnchorOffset<Content>(new Vector2(0, 0), new Vector2(x.CurrentValue<float>(), 1f),
                Vector2.zero, Vector2.zero)
        );

        _progressContent?.SetAnchorOffset<Content>(new Vector2(0, 0), new Vector2(currentStatus.Progress, 1f),
            Vector2.zero, Vector2.zero);
        _statusLabel?.SetText(currentStatus.Message);
    }

    private float _lastUpdate;
    private bool _isClosing;

    public override void Update() {

        if (_isClosing)
            return;
        
        if (Time.realtimeSinceStartup - _lastUpdate > UPDATE_INTERVAL_SEC) {
            _lastUpdate = Time.realtimeSinceStartup;
            UpdateStatusLabel();
        }

        if (_networkTask.IsCompleted) {
            _isClosing = DynamicUIManager.CloseActiveModal();
        }
    }
    
    public override void Delete() {
        SynthesisTween.CancelTween(PROGRESS_BAR_TWEEN);
        _callback?.Invoke();
    }
}
