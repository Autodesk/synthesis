using System;
using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class BetaWarningPanel : PanelDynamic {
    private const string DISPLAY_WARNING_PREF = "beta/warning";

    private Action _onOk;

    public BetaWarningPanel(Action onOk) : base(new Vector2(350, 100)) {
        _onOk = onOk;
    }

    public override bool Create() {
        if (!PreferenceManager.ContainsPreference(DISPLAY_WARNING_PREF))
            PreferenceManager.SetPreference<bool>(DISPLAY_WARNING_PREF, true);
        PreferenceManager.Save();

        if (!PreferenceManager.GetPreference<bool>(DISPLAY_WARNING_PREF))
            return false;

        Title.SetText("Warning");
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.ClosePanel<BetaWarningPanel>())
            .StepIntoLabel(l => l.SetText("Okidoki"));
        CancelButton.RootGameObject.SetActive(false);
        PanelImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"));
        PanelImage.SetColor(ColorManager.SYNTHESIS_WHITE);

        MainContent.CreateLabel(40)
            .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
            .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
            .SetAnchoredPosition<Label>(new Vector2(0, 15))
            .SetText("This feature you are about to use is in beta and is not unlikely to do funky things.")
            .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
            .SetFontSize(20);

        return true;
    }

    public override void Update() {}

    public override void Delete() {
        PreferenceManager.SetPreference<bool>(DISPLAY_WARNING_PREF, false);
        PreferenceManager.Save();
        _onOk();
    }
}
