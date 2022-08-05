using System.Collections;
using System.Collections.Generic;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

using Image = UnityEngine.UI.Image;

public class TopBar : MonoBehaviour {
    // TODO: Change colors of other stuff on the top bar
    public void Start() {
        GetComponent<Image>().color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK);
    }

    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }
}
