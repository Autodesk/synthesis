using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;
using Image = UnityEngine.UI.Image;

public class TopBar : MonoBehaviour {
    public void Start() {
        AssignColors();

        EventBus.NewTypeListener<ColorManager.OnThemeChanged>(x => { AssignColors(); });
    }

    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }

    private void AssignColors() {
        GetComponent<Image>().color = ColorManager.GetColor(ColorManager.SynthesisColor.Background);
        transform.Find("ExitButton").GetComponent<Image>().color =
            ColorManager.GetColor(ColorManager.SynthesisColor.MainText);
        transform.Find("VersionNumber").GetComponent<TextMeshProUGUI>().color =
            ColorManager.GetColor(ColorManager.SynthesisColor.MainText);
    }
}
