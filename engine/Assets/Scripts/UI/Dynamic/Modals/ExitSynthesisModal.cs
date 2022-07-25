using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class ExitSynthesisModal : ModalDynamic {
        public ExitSynthesisModal() : base(new Vector2(350, 50)) { }

        public override void Create() {
            Title.SetText("Exit Synthesis");
            Description.SetText("");
            AcceptButton.AddOnClickedEvent(x => Application.Quit()).StepIntoLabel(l => l.SetText("Exit"));

            MainContent.CreateLabel(40)
                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Top)
                .SetText("Are you sure you wish to Exit?")
                .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
                .SetFontSize(20);
        }

        public override void Update() { }
        public override void Delete() { }
    }
}
