using UnityEngine;

namespace Utilities.ColorManager {
    /// <summary>Colors of the default synthesis theme</summary>
    public static class DefaultColors {
        public static readonly(ColorManager.SynthesisColor name, Color32 color)[] SYNTHESIS_DEFAULT = {
            (ColorManager.SynthesisColor.InteractiveElementSolid, new Color32(250, 162, 27, 255)),
            (ColorManager.SynthesisColor.InteractiveElementLeft, new Color32(224, 130, 65, 255)),
            (ColorManager.SynthesisColor.InteractiveElementRight, new Color32(218, 102, 89, 255)),
            (ColorManager.SynthesisColor.InteractiveSecondary, new Color32(204, 124, 0, 255)),
            (ColorManager.SynthesisColor.Background, new Color32(33, 37, 41, 255)),
            (ColorManager.SynthesisColor.BackgroundSecondary, new Color32(52, 58, 64, 255)),
            (ColorManager.SynthesisColor.MainText, new Color32(248, 249, 250, 255)),
            (ColorManager.SynthesisColor.Scrollbar, new Color32(213, 216, 223, 255)),
            (ColorManager.SynthesisColor.AcceptButton, new Color32(34, 139, 230, 255)),
            (ColorManager.SynthesisColor.CancelButton, new Color32(250, 82, 82, 255)),
            (ColorManager.SynthesisColor.InteractiveElementText, new Color32(0, 0, 0, 255)),
            (ColorManager.SynthesisColor.Icon, new Color32(255, 255, 255, 255)),
            (ColorManager.SynthesisColor.HighlightHover, new Color32(89, 255, 133, 255)),
            (ColorManager.SynthesisColor.HighlightSelect, new Color32(255, 89, 133, 255)),
            (ColorManager.SynthesisColor.SkyboxTop, new Color32(255, 255, 255, 255)),
            (ColorManager.SynthesisColor.SkyboxBottom, new Color32(255, 255, 255, 255)),
            (ColorManager.SynthesisColor.FloorGrid, new Color32(93, 93, 93, 255))
        };
    }
}