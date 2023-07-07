using System.Collections;
using System.Collections.Generic;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;
using Image = UnityEngine.UI.Image;

public class TopBar : MonoBehaviour {
    // TODO: Change colors of other stuff on the top bar
    public void Start() {
        GetComponent<Image>().color = ColorManager.GetColor(ColorManager.SynthesisColor.SynthesisBlack);
    }

    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }
}
