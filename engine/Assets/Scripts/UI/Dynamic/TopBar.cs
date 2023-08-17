using System;
using JetBrains.Annotations;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;
using Image = UnityEngine.UI.Image;

public class TopBar : MonoBehaviour {
    public void Start() {
        AssignColors(null);

        EventBus.NewTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }

    private void OnDestroy() {
        EventBus.RemoveTypeListener<ColorManager.OnThemeChanged>(AssignColors);
    }

    private void AssignColors(IEvent e) {
        GetComponent<Image>().color = ColorManager.GetColor(ColorManager.SynthesisColor.Background);
        transform.Find("ExitButton").GetComponent<Image>().color =
            ColorManager.GetColor(ColorManager.SynthesisColor.MainText);
        transform.Find("VersionNumber").GetComponent<TextMeshProUGUI>().color =
            ColorManager.GetColor(ColorManager.SynthesisColor.MainText);
    }
}
