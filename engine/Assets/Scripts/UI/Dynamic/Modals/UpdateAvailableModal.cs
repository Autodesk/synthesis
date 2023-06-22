using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class UpdateAvailableModal : ModalDynamic {

        public UpdateAvailableModal() : base(new Vector2(350, 50)) {
        }

        public override void Create() {

            Title.SetText("Exit Synthesis");
            Description.SetText("");
            AcceptButton
                .AddOnClickedEvent(x => {
                    Process.Start(
                        new ProcessStartInfo() { FileName = AutoUpdater.UpdaterLink, UseShellExecute = true });
                })
                .StepIntoLabel(l => l.SetText("Yes Please"))
                .SetPivot<Button>(new Vector2(1.0f, 0.0f))
                .SetWidth<Button>(125);
            CancelButton.StepIntoLabel(l => l.SetText("I'm Good"))
                .SetPivot<Button>(new Vector2(0.0f, 0.0f))
                .SetWidth<Button>(125);
            ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"));
            ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

            MainContent.CreateLabel(40)
                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                .SetAnchoredPosition<Label>(new Vector2(0, 15))
                .SetText("A new update is available\nWould you like to update?")
                .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
                .SetFontSize(20);
        }

        public override void Update() {
        }
        public override void Delete() {
        }
    }
}
