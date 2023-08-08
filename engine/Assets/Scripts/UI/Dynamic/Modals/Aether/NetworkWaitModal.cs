using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using SynthesisAPI.Utilities;
using UnityEngine;
using Utilities.ColorManager;

public class NetworkWaitModal : ModalDynamic {

    private const float MODAL_WIDTH = 300;
    private const float MODAL_HEIGHT = 300;
    private const float EDGE_PADDING = 10f;
    private const float UPDATE_INTERVAL_SEC = 0.25f;

    private Task<bool> _task;
    private AtomicReadOnly<NetworkTaskStatus> _status;

    private Label _statusLabel;
    private Content _progressContent;

    public NetworkWaitModal(Task<bool> task, AtomicReadOnly<NetworkTaskStatus> status)
        : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _task = task;
        _status = status;
    }
    
    public override void Create() {
        var newContent = Strip(newContentSize: new Vector2(MODAL_WIDTH, MODAL_HEIGHT), leftPadding: EDGE_PADDING,
            rightPadding: EDGE_PADDING, topPadding: EDGE_PADDING, bottomPadding: EDGE_PADDING);
        _statusLabel = newContent.CreateLabel().SetStretch(bottomPadding: 10f).SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
            .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center).SetFontSize(20f);

        var container = newContent.CreateSubContent(new Vector2(20f, 10f)).SetBottomStretch<Content>().EnsureImage().StepIntoImage(
            i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.BackgroundSecondary));
		_progressContent = container.CreateSubContent(new Vector2(20f, 10f)).SetStretch<Content>().EnsureImage().StepIntoImage(
			i => i.SetSprite(null).SetColor(ColorManager.SynthesisColor.InteractiveElementLeft, ColorManager.SynthesisColor.InteractiveElementRight));

        UpdateStatusLabel();
	}

    private void UpdateStatusLabel() {
        var currentStatus = _status.Value;
        _progressContent.SetAnchorOffset<Content>(new Vector2(0, 0), new Vector2(currentStatus.Progress, 1f),
            Vector2.zero, Vector2.zero);
        _statusLabel.SetText(currentStatus.Message);
    }

    private float _lastUpdate = 0f;

    public override void Update() {
        if (Time.realtimeSinceStartup - _lastUpdate > UPDATE_INTERVAL_SEC) {
            _lastUpdate = Time.realtimeSinceStartup;
            UpdateStatusLabel();
        }

        if (_task.IsCompleted) {
            DynamicUIManager.CloseActiveModal();
        }
    }
    public override void Delete() { }
}
